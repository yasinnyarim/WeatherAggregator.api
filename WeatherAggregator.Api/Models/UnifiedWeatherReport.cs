using System.Xml.Serialization;

namespace WeatherAggregator.Api.Models
{
    [XmlRoot("WeatherReport")]
    public class UnifiedWeatherReport
    {
        [XmlElement("City")]
        public string City { get; set; } = string.Empty;
        [XmlElement("Temperature")]
        public double Temperature { get; set; }
        [XmlElement("Unit")]
        public string Unit { get; set; } = string.Empty;
        [XmlElement("Condition")]
        public string Condition { get; set; } = string.Empty;
        [XmlElement("Humidity")]
        public double Humidity { get; set; }
        [XmlElement("WindSpeed")]
        public double WindSpeed { get; set; }
        [XmlElement("Source")]
        public string Source { get; set; } = string.Empty;
    }
}