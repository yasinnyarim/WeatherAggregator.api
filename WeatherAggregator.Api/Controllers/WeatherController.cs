using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public WeatherController(GeocodingService g, MetNorwayService m, XsltTransformService x)
        {
            _geocodingService = g; _metNorwayService = m; _xsltTransformService = x;
        }

        [HttpGet("{city}"), Authorize, Produces("application/xml")]
        public async Task<IActionResult> GetWeather(string city)
        {
            var report = await GetWeatherReport(city);
            if (report == null) return StatusCode(500, "Failed to get data.");
            return Ok(report);
        }

        [HttpGet("{city}/report"), Authorize, Produces("text/html")]
        public async Task<IActionResult> GetWeatherReportAsHtml(string city)
        {
            var report = await GetWeatherReport(city);
            if (report == null) return Content("Error: Data not found.", "text/html");

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

        private async Task<UnifiedWeatherReport?> GetWeatherReport(string city)
        {
            var coords = await _geocodingService.GetCoordinatesAsync(city);
            if (coords == null) return null;
            return await _metNorwayService.GetWeatherAsync(city, coords.Value.Latitude, coords.Value.Longitude);
        }
    }
}