using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;
using shrey_st10438635_PROG7311.Services;
using Xunit;

namespace shrey_st10438635_PROG7311_Tests
{
    /// <summary>
    /// Unit tests for ContractRepository using EF Core InMemory database.
    /// Tests LINQ filtering logic for the Admin search feature.
    /// </summary>
    public class ContractRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ContractRepository _repo;

        public ContractRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            _context = new ApplicationDbContext(options);
            _repo = new ContractRepository(_context);

            SeedData();
        }

        private void SeedData()
        {
            var client = new Client { Id = 10, Name = "Test Co", ContactEmail = "t@t.com", ContactPhone = "111", Region = "Africa" };
            _context.Clients.Add(client);

            _context.Contracts.AddRange(
                new Contract { Id = 100, ClientId = 10, Title = "Active 2024", StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = ContractStatus.Active, ServiceLevel = ServiceLevel.Standard, CreatedAt = DateTime.Now },
                new Contract { Id = 101, ClientId = 10, Title = "Expired 2023", StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2023, 12, 31), Status = ContractStatus.Expired, ServiceLevel = ServiceLevel.Express, CreatedAt = DateTime.Now },
                new Contract { Id = 102, ClientId = 10, Title = "Draft 2025", StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2027, 6, 1), Status = ContractStatus.Draft, ServiceLevel = ServiceLevel.Premium, CreatedAt = DateTime.Now }
            );
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllContracts()
        {
            var results = await _repo.GetAllAsync();
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsContract()
        {
            var result = await _repo.GetByIdAsync(100);
            Assert.NotNull(result);
            Assert.Equal("Active 2024", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            var result = await _repo.GetByIdAsync(9999);
            Assert.Null(result);
        }

        [Fact]
        public async Task FilterAsync_ByActiveStatus_ReturnsOnlyActive()
        {
            var results = await _repo.FilterAsync(null, null, ContractStatus.Active);
            Assert.All(results, c => Assert.Equal(ContractStatus.Active, c.Status));
            Assert.Single(results);
        }

        [Fact]
        public async Task FilterAsync_ByExpiredStatus_ReturnsOnlyExpired()
        {
            var results = await _repo.FilterAsync(null, null, ContractStatus.Expired);
            Assert.Single(results);
            Assert.Equal("Expired 2023", results[0].Title);
        }

        [Fact]
        public async Task FilterAsync_ByDateRange_ReturnsCorrectContracts()
        {
            // Only contracts starting on/after 2024-01-01 (excludes "Expired 2023" which starts 2023-01-01)
            var results = await _repo.FilterAsync(new DateTime(2024, 1, 1), null, null);
            Assert.Equal(2, results.Count); // "Active 2024" + "Draft 2025"
            Assert.DoesNotContain(results, c => c.Title == "Expired 2023");
        }

        [Fact]
        public async Task FilterAsync_NoFilters_ReturnsAll()
        {
            var results = await _repo.FilterAsync(null, null, null);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task GetActiveContractsAsync_ReturnsOnlyActive()
        {
            var results = await _repo.GetActiveContractsAsync();
            Assert.All(results, c => Assert.Equal(ContractStatus.Active, c.Status));
        }

        [Fact]
        public async Task AddAsync_NewContract_IncreasesCount()
        {
            var newContract = new Contract
            {
                ClientId = 10,
                Title = "New Test",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = ContractStatus.Draft,
                ServiceLevel = ServiceLevel.Standard,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(newContract);
            await _repo.SaveAsync();

            var all = await _repo.GetAllAsync();
            Assert.Equal(4, all.Count);
        }

        [Fact]
        public async Task DeleteAsync_ExistingContract_RemovesIt()
        {
            await _repo.DeleteAsync(102);
            await _repo.SaveAsync();

            var result = await _repo.GetByIdAsync(102);
            Assert.Null(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}