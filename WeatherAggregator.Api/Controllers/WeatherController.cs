using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using WeatherAggregator.Api.Models;
using WeatherAggregator.Api.Services;

namespace WeatherAggregator.Api.Controllers
{
    [Route("api/v1/[controller]"), ApiController, Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly GeocodingService _geo;
        private readonly MetNorwayService _met;
        private readonly XsltTransformService _xslt;
        private readonly XmlValidationService _xmlValidation;
        public WeatherController(GeocodingService g, MetNorwayService m, XsltTransformService x, XmlValidationService v)
        { _geo = g; _met = m; _xslt = x; _xmlValidation = v; }

        [HttpGet("{city}"), Produces("application/xml")]
        public async Task<IActionResult> GetWeather(string city)
        {
            var report = await GetWeatherReport(city);
            if (report == null) return StatusCode(500, "Failed to get data.");
            return Ok(report);
        }

        [HttpGet("{city}/report"), Produces("text/html")]
        public async Task<IActionResult> GetWeatherReportAsHtml(string city)
        {
            var report = await GetWeatherReport(city);
            if (report == null) return Content("Error: Data not found.", "text/html");
            var serializer = new XmlSerializer(typeof(UnifiedWeatherReport));
            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, report);
            return Content(_xslt.TransformXml(stringWriter.ToString()), "text/html");
        }

        [HttpPost("validate/xsd"), Consumes("application/xml")]
        public async Task<IActionResult> ValidateWithXsd()
        {
            var (isValid, errors) = _xmlValidation.ValidateWithXsd(await new StreamReader(Request.Body).ReadToEndAsync());
            if (!isValid) return BadRequest(new { message = "XSD validation failed.", validationErrors = errors });
            return Ok(new { message = "XML is valid according to XSD." });
        }

        [HttpPost("validate/dtd"), Consumes("application/xml")]
        public async Task<IActionResult> ValidateWithDtd()
        {
            var (isValid, errors) = _xmlValidation.ValidateWithDtd(await new StreamReader(Request.Body).ReadToEndAsync());
            if (!isValid) return BadRequest(new { message = "DTD validation failed.", validationErrors = errors });
            return Ok(new { message = "XML is valid according to DTD." });
        }

        private async Task<UnifiedWeatherReport?> GetWeatherReport(string city)
        {
            var coords = await _geo.GetCoordinatesAsync(city);
            if (coords == null) return null;
            return await _met.GetWeatherAsync(city, coords.Value.Latitude, coords.Value.Longitude);
        }
    }
}