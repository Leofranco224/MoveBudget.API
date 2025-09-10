using System.Text.Json;

namespace MoveBudget.API.Services
{
    public class CurrencyConversionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CurrencyConversionService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["ExchangeRateApi:BaseUrl"]!);
            _apiKey = config["ExchangeRateApi:ApiKey"]!;
        }

        public async Task<decimal?> ConvertAsync(string from, string to, decimal amount)
        {
            var response = await _httpClient.GetAsync(
                $"convert?access_key={_apiKey}&from={from}&to={to}&amount={amount}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("result", out var resultElement))
                return resultElement.GetDecimal();

            return null;
        }
    }
}