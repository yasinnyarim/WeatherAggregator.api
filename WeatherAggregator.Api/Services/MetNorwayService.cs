using System.Globalization;
using System.Text.Json;
using WeatherAggregator.Api.Models;
namespace WeatherAggregator.Api.Services
{
    public class MetNorwayService
    {
        private class WeatherData { public Properties? properties { get; set; } }
        private class Properties { public List<Timeseries>? timeseries { get; set; } }
        private class Timeseries { public Data? data { get; set; } }
        private class Data { public Instant? instant { get; set; } public Next1Hours? next_1_hours { get; set; } }
        private class Instant { public Details? details { get; set; } }
        private class Details { public double? air_temperature { get; set; } public double? relative_humidity { get; set; } public double? wind_speed { get; set; } }
        private class Next1Hours { public Summary? summary { get; set; } }
        private class Summary { public string? symbol_code { get; set; } }

        private readonly IHttpClientFactory _httpClientFactory;
        public MetNorwayService(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

        public async Task<UnifiedWeatherReport?> GetWeatherAsync(string city, double latitude, double longitude)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "YasinAirWeatherApp/1.0 (contact@example.com)");
            var url = $"https://api.met.no/weatherapi/locationforecast/2.0/compact?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}";
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                var jsonString = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<WeatherData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var firstTimeData = weatherData?.properties?.timeseries?.FirstOrDefault()?.data;
                if (firstTimeData?.instant?.details == null) return null;
                return new UnifiedWeatherReport
                {
                    City = char.ToUpper(city[0]) + city.Substring(1),
                    Temperature = firstTimeData.instant.details.air_temperature ?? 0.0,
                    Unit = "Celsius",
                    Condition = firstTimeData.next_1_hours?.summary?.symbol_code ?? "Unknown",
                    Humidity = firstTimeData.instant.details.relative_humidity ?? 0.0,
                    WindSpeed = firstTimeData.instant.details.wind_speed ?? 0.0,
                    Source = "MET Norway (from JSON)"
                };
            }
            catch { return null; }
        }
    }
}