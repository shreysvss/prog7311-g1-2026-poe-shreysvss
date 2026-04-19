using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using shrey_st10438635_PROG7311.Services;
using Xunit;

// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <xUnit.net, 2026. Getting Started with xUnit.net. [online] Available at: https://xunit.net/docs/getting-started/netcore/cmdline [Accessed 15 April 2026].>
// <Code Maze, 2026. Mocking in Unit Tests with Moq. [online] Available at: https://code-maze.com/moq-tutorial [Accessed 16 April 2026].>
// <GeeksforGeeks, 2026. Unit Testing in C# using xUnit. [online] Available at: https://www.geeksforgeeks.org/unit-testing-in-c-sharp [Accessed 17 April 2026].>
// <Tutorials Teacher, 2026. EF Core InMemory Database Provider for Testing. [online] Available at: https://www.tutorialsteacher.com/core/ef-core-testing [Accessed 18 April 2026].>

namespace shrey_st10438635_PROG7311_Tests
{
    /// <summary>
    /// Unit tests for CurrencyService — verifies USD→ZAR conversion math.
    /// Rubric: "Currency Calculation: Verify that the math converting USD to ZAR is correct, given a specific rate."
    /// </summary>
    public class CurrencyServiceTests
    {
        private readonly CurrencyService _service;

        public CurrencyServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockConfig = new Mock<IConfiguration>();
            _service = new CurrencyService(mockFactory.Object, mockLogger.Object, mockConfig.Object);
        }

        // Happy Path Tests 

        [Fact]
        public void ConvertUsdToZar_CorrectMath_ReturnsExpected()
        {
            // Arrange
            decimal usdAmount = 100m;
            decimal rate = 18.50m;
            decimal expected = 1850.00m;

            // Act
            decimal result = _service.ConvertUsdToZar(usdAmount, rate);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertUsdToZar_RoundsToTwoDecimalPlaces()
        {
            // Arrange — rate produces many decimals
            decimal usdAmount = 1m;
            decimal rate = 18.12345m;
            decimal expected = Math.Round(1m * 18.12345m, 2); // 18.12

            // Act
            decimal result = _service.ConvertUsdToZar(usdAmount, rate);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertUsdToZar_ZeroAmount_ReturnsZero()
        {
            // Arrange
            decimal usdAmount = 0m;
            decimal rate = 18.50m;

            // Act
            decimal result = _service.ConvertUsdToZar(usdAmount, rate);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void ConvertUsdToZar_LargeAmount_CalculatesCorrectly()
        {
            // Arrange — simulate 50,000 contracts scenario
            decimal usdAmount = 50000m;
            decimal rate = 18.50m;
            decimal expected = 925000.00m;

            // Act
            decimal result = _service.ConvertUsdToZar(usdAmount, rate);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100, 18.50, 1850.00)]
        [InlineData(250, 17.80, 4450.00)]
        [InlineData(1000, 19.00, 19000.00)]
        [InlineData(0.01, 18.50, 0.18)]
        public void ConvertUsdToZar_MultipleRates_AllCorrect(decimal usd, decimal rate, decimal expected)
        {
            // Act
            decimal result = _service.ConvertUsdToZar(usd, rate);

            // Assert
            Assert.Equal(expected, result);
        }

        //Edge Case / Failure Tests 

        [Fact]
        public void ConvertUsdToZar_ZeroRate_ThrowsArgumentException()
        {
            // Arrange
            decimal usdAmount = 100m;
            decimal rate = 0m;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.ConvertUsdToZar(usdAmount, rate));
            Assert.Contains("Exchange rate must be greater than zero", ex.Message);
        }

        [Fact]
        public void ConvertUsdToZar_NegativeRate_ThrowsArgumentException()
        {
            // Arrange
            decimal usdAmount = 100m;
            decimal rate = -5m;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.ConvertUsdToZar(usdAmount, rate));
        }

        [Fact]
        public void ConvertUsdToZar_NegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            decimal usdAmount = -50m;
            decimal rate = 18.50m;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.ConvertUsdToZar(usdAmount, rate));
            Assert.Contains("Amount cannot be negative", ex.Message);
        }
    }
}