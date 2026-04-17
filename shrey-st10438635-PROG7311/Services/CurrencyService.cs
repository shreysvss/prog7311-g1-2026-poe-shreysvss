namespace shrey_st10438635_PROG7311.Services
{
    // ─── Interface (Design Pattern: Strategy / Dependency Inversion) ─────────────
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();
        decimal ConvertUsdToZar(decimal usdAmount, decimal rate);
    }

    // ─── Implementation ───────────────────────────────────────────────────────────
    public class CurrencyService : ICurrencyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IConfiguration _configuration;

        // Simple in-memory cache to avoid hammering the free API
        private decimal _cachedRate = 0;
        private DateTime _cacheTime = DateTime.MinValue;
        private const int CacheMinutes = 30;

        public CurrencyService(IHttpClientFactory httpClientFactory,
                               ILogger<CurrencyService> logger,
                               IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // Return cached rate if still valid
            if (_cachedRate > 0 && (DateTime.UtcNow - _cacheTime).TotalMinutes < CacheMinutes)
                return _cachedRate;

            try
            {
                var client = _httpClientFactory.CreateClient("CurrencyClient");

                // Using open.er-api.com — free, no API key needed
                var response = await client.GetAsync("https://open.er-api.com/v6/latest/USD");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var rates = doc.RootElement.GetProperty("rates");
                var zarRate = rates.GetProperty("ZAR").GetDecimal();

                _cachedRate = zarRate;
                _cacheTime = DateTime.UtcNow;

                return zarRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch exchange rate. Using fallback rate.");
                // Fallback rate if API is unavailable
                return 18.50m;
            }
        }

        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
        {
            if (rate <= 0) throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));
            if (usdAmount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(usdAmount));
            return Math.Round(usdAmount * rate, 2);
        }
    }
}
