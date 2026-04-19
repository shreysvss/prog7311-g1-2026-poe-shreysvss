using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;

// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <Code Maze, 2026. Repository Pattern with ASP.NET Core and Entity Framework. [online] Available at: https://code-maze.com/the-repository-pattern-aspnet-core [Accessed 15 April 2026].>
// <Refactoring Guru, 2026. Strategy Design Pattern. [online] Available at: https://refactoring.guru/design-patterns/strategy [Accessed 16 April 2026].>
// <Tutorials Teacher, 2026. Consuming a Web API using HttpClient. [online] Available at: https://www.tutorialsteacher.com/core/consume-web-api-httpclient [Accessed 17 April 2026].>
// <GeeksforGeeks, 2026. async and await in C#. [online] Available at: https://www.geeksforgeeks.org/async-and-await-in-c-sharp [Accessed 18 April 2026].>

namespace shrey_st10438635_PROG7311.Services
{
    // Repository Pattern (GoF / architectural pattern) 
    public interface IContractRepository
    {
        Task<List<Contract>> GetAllAsync();
        Task<Contract?> GetByIdAsync(int id);
        Task<List<Contract>> FilterAsync(DateTime? startFrom, DateTime? startTo, ContractStatus? status);
        Task<List<Contract>> GetActiveContractsAsync();
        Task AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }

    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Contract>> GetAllAsync()
            => await _context.Contracts.Include(c => c.Client).OrderByDescending(c => c.CreatedAt).ToListAsync();

        public async Task<Contract?> GetByIdAsync(int id)
            => await _context.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<List<Contract>> FilterAsync(DateTime? startFrom, DateTime? startTo, ContractStatus? status)
        {
            // LINQ filtering
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (startFrom.HasValue)
                query = query.Where(c => c.StartDate >= startFrom.Value);

            if (startTo.HasValue)
                query = query.Where(c => c.StartDate <= startTo.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<List<Contract>> GetActiveContractsAsync()
            => await _context.Contracts.Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

        public async Task AddAsync(Contract contract) => await _context.Contracts.AddAsync(contract);

        public async Task UpdateAsync(Contract contract) => _context.Contracts.Update(contract);

        public async Task DeleteAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null) _context.Contracts.Remove(contract);
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
