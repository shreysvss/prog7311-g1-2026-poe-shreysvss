//Code attribution
//Title: Consuming a Web API using HttpClient
//Author: Tutorials Teacher
//Date: 17 April 2026
//Version: 1
//Availability: https://www.tutorialsteacher.com/core/consume-web-api-httpclient

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


namespace shrey_st10438635_PROG7311.Services
{
    // The interface that defines what a currency service must be able to do
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();
        Task<decimal> GetRateToZarAsync(string sourceCurrency);
        decimal ConvertUsdToZar(decimal usdAmount, decimal rate);
        decimal ConvertToZar(decimal amount, decimal rate);
        IReadOnlyList<string> SupportedCurrencies { get; }
    }

    // Fetches live exchange rates from an external API and converts amounts to ZAR
    public class CurrencyService : ICurrencyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IConfiguration _configuration;

        // The list of currencies the app accepts — the dropdown in the UI uses this list
        private static readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "AUD", "CAD", "JPY" };

        // Rough fallback rates in case the external API is down, so the feature never fully breaks
        private static readonly Dictionary<string, decimal> _fallbackRates = new()
        {
            { "USD", 18.50m },
            { "EUR", 20.00m },
            { "GBP", 23.50m },
            { "AUD", 12.00m },
            { "CAD", 13.50m },
            { "JPY", 0.12m }
        };

        // Stores the last fetched rate per currency so we don't hit the free API on every page load
        private readonly Dictionary<string, (decimal rate, DateTime time)> _cache = new();
        private const int CacheMinutes = 30;

        public CurrencyService(IHttpClientFactory httpClientFactory,
                               ILogger<CurrencyService> logger,
                               IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public IReadOnlyList<string> SupportedCurrencies => _supportedCurrencies;

        // Kept around so the original USD-only unit tests still work
        public async Task<decimal> GetUsdToZarRateAsync() => await GetRateToZarAsync("USD");

        // Fetches the live exchange rate from the given source currency into ZAR
        public async Task<decimal> GetRateToZarAsync(string sourceCurrency)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency))
                throw new ArgumentException("Source currency is required.", nameof(sourceCurrency));

            sourceCurrency = sourceCurrency.ToUpperInvariant();

            if (!_supportedCurrencies.Contains(sourceCurrency))
                throw new ArgumentException($"Currency '{sourceCurrency}' is not supported.", nameof(sourceCurrency));

            // If we already have a recent cached rate for this currency, use it
            if (_cache.TryGetValue(sourceCurrency, out var cached)
                && cached.rate > 0
                && (DateTime.UtcNow - cached.time).TotalMinutes < CacheMinutes)
            {
                return cached.rate;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CurrencyClient");

                // Ask the API for the latest rates, using our source currency as the base
                var response = await client.GetAsync($"https://open.er-api.com/v6/latest/{sourceCurrency}");
                response.EnsureSuccessStatusCode();

                // Parse the JSON response and pull out the ZAR rate
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var rates = doc.RootElement.GetProperty("rates");
                var zarRate = rates.GetProperty("ZAR").GetDecimal();

                // Save the rate in the cache for next time
                _cache[sourceCurrency] = (zarRate, DateTime.UtcNow);
                return zarRate;
            }
            catch (Exception ex)
            {
                // If anything goes wrong, log it and fall back to the default rate
                _logger.LogError(ex, "Failed to fetch {Currency}->ZAR rate. Using fallback.", sourceCurrency);
                return _fallbackRates[sourceCurrency];
            }
        }

        // Kept around so the original USD-only unit tests still work
        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate) => ConvertToZar(usdAmount, rate);

        // Multiplies the amount by the rate and rounds to 2 decimal places
        public decimal ConvertToZar(decimal amount, decimal rate)
        {
            if (rate <= 0) throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            return Math.Round(amount * rate, 2);
        }
    }
}