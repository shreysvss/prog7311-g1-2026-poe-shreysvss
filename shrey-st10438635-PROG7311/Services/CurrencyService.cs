namespace shrey_st10438635_PROG7311.Services

// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <Code Maze, 2026. Repository Pattern with ASP.NET Core and Entity Framework. [online] Available at: https://code-maze.com/the-repository-pattern-aspnet-core [Accessed 15 April 2026].>
// <Refactoring Guru, 2026. Strategy Design Pattern. [online] Available at: https://refactoring.guru/design-patterns/strategy [Accessed 16 April 2026].>
// <Tutorials Teacher, 2026. Consuming a Web API using HttpClient. [online] Available at: https://www.tutorialsteacher.com/core/consume-web-api-httpclient [Accessed 17 April 2026].>
// <GeeksforGeeks, 2026. async and await in C#. [online] Available at: https://www.geeksforgeeks.org/async-and-await-in-c-sharp [Accessed 18 April 2026].>

{
    // Interface (Strategy / Dependency Inversion) (Code Maze, 2026)
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();
        Task<decimal> GetRateToZarAsync(string sourceCurrency);
        decimal ConvertUsdToZar(decimal usdAmount, decimal rate);
        decimal ConvertToZar(decimal amount, decimal rate);
        IReadOnlyList<string> SupportedCurrencies { get; }
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IConfiguration _configuration;

        // Supported source currencies — dropdown in the UI mirrors this list
        private static readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "AUD", "CAD", "JPY" };

        // Fallback rates if API is unavailable (rough approximations to ZAR)
        private static readonly Dictionary<string, decimal> _fallbackRates = new()
        {
            { "USD", 18.50m },
            { "EUR", 20.00m },
            { "GBP", 23.50m },
            { "AUD", 12.00m },
            { "CAD", 13.50m },
            { "JPY", 0.12m }
        };

        // Simple in-memory cache per currency, to avoid hammering the free API  (GeeksforGeeks, 2026)
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

        // Kept for backwards compatibility with existing tests
        public async Task<decimal> GetUsdToZarRateAsync() => await GetRateToZarAsync("USD");

        public async Task<decimal> GetRateToZarAsync(string sourceCurrency)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency))
                throw new ArgumentException("Source currency is required.", nameof(sourceCurrency));

            sourceCurrency = sourceCurrency.ToUpperInvariant();

            if (!_supportedCurrencies.Contains(sourceCurrency))
                throw new ArgumentException($"Currency '{sourceCurrency}' is not supported.", nameof(sourceCurrency));

            // Return cached rate if still valid
            if (_cache.TryGetValue(sourceCurrency, out var cached)
                && cached.rate > 0
                && (DateTime.UtcNow - cached.time).TotalMinutes < CacheMinutes)
            {
                return cached.rate;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CurrencyClient");

                // open.er-api.com returns all rates against the base currency
                var response = await client.GetAsync($"https://open.er-api.com/v6/latest/{sourceCurrency}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var rates = doc.RootElement.GetProperty("rates");
                var zarRate = rates.GetProperty("ZAR").GetDecimal();

                _cache[sourceCurrency] = (zarRate, DateTime.UtcNow);
                return zarRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch {Currency}->ZAR rate. Using fallback.", sourceCurrency);
                return _fallbackRates[sourceCurrency];
            }
        }

        // Kept for backwards compatibility with existing tests
        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate) => ConvertToZar(usdAmount, rate);

        public decimal ConvertToZar(decimal amount, decimal rate)
        {
            if (rate <= 0) throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            return Math.Round(amount * rate, 2);
        }
    }
}