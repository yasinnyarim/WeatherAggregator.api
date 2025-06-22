using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using WeatherAggregator.Api.Models;
using WeatherAggregator.Api.Services;

namespace WeatherAggregator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly GeocodingService _geocodingService;
        private readonly MetNorwayService _metNorwayService;
        private readonly XsltTransformService _xsltTransformService;
        private readonly XmlValidationService _xmlValidationService;

        public WeatherController(
            GeocodingService geocodingService,
            MetNorwayService metNorwayService,
            XsltTransformService xsltTransformService,
            XmlValidationService xmlValidationService)
        {
            _geocodingService = geocodingService;
            _metNorwayService = metNorwayService;
            _xsltTransformService = xsltTransformService;
            _xmlValidationService = xmlValidationService;
        }

        [HttpGet("{city}")]
        [Authorize]
        [Produces("application/xml")]
        public async Task<IActionResult> GetWeather(string city)
        {
            var report = await GetWeatherReport(city);
            if (report == null)
            {
                return StatusCode(500, "Failed to retrieve or process weather data.");
            }
            return Ok(report);
        }

        [HttpGet("{city}/report")]
        [Authorize]
        [Produces("text/html")]
        public async Task<IActionResult> GetWeatherReportAsHtml(string city)
        {
            var report = await GetWeatherReport(city);
            if (report == null)
            {
                return Content("Error: Data not found.", "text/html");
            }

            string xmlContent;
            var serializer = new XmlSerializer(typeof(UnifiedWeatherReport));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, report);
                xmlContent = stringWriter.ToString();
            }

            string htmlContent = _xsltTransformService.TransformXml(xmlContent);
            return Content(htmlContent, "text/html");
        }

        // === XML DOĞRULAMA ENDPOINT'İ (DÜZELTİLMİŞ HALİ) ===
        [HttpPost("validate")]
        [Authorize]
        [Consumes("application/xml")]
        public async Task<IActionResult> ValidateWeatherReport()
        {
            string xmlContent;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                xmlContent = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(xmlContent))
            {
                return BadRequest(new { message = "Request body cannot be empty." });
            }

            // Yeni Validate metodunu çağırıyoruz.
            var (isValid, errors) = _xmlValidationService.Validate(xmlContent);

            if (!isValid)
            {
                return BadRequest(new { message = "XML validation failed.", validationErrors = errors });
            }

            try
            {
                var serializer = new XmlSerializer(typeof(UnifiedWeatherReport));
                using var stringReader = new StringReader(xmlContent);
                var report = (UnifiedWeatherReport?)serializer.Deserialize(stringReader);

                return Ok(new { message = "XML is valid and successfully deserialized.", receivedCity = report?.City });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"XML was valid but could not be deserialized. Error: {ex.Message}");
            }
        }

        // Yardımcı metod (kod tekrarını önler)
        private async Task<UnifiedWeatherReport?> GetWeatherReport(string city)
        {
            var coordinates = await _geocodingService.GetCoordinatesAsync(city);
            if (coordinates == null) return null;
            return await _metNorwayService.GetWeatherAsync(city, coordinates.Value.Latitude, coordinates.Value.Longitude);
        }
    }
}