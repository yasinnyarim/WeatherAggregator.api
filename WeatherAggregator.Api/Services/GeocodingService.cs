using System.Text.Json;
namespace WeatherAggregator.Api.Services
{
    public class GeocodingResult
    {
        public class ResultItem { public double latitude { get; set; } public double longitude { get; set; } }
        public ResultItem[] results { get; set; } = Array.Empty<ResultItem>();
    }

    public class GeocodingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public GeocodingService(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string city)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={city}&count=1&language=en&format=json";
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GeocodingResult>(jsonString);
                    if (result?.results != null && result.results.Length > 0)
                    {
                        return (result.results[0].latitude, result.results[0].longitude);
                    }
                }
            }
            catch { }
            return null;
        }
    }
}