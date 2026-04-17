using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;

namespace shrey_st10438635_PROG7311.Services
{
    // ─── Repository Pattern (GoF / architectural pattern) ────────────────────────
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
