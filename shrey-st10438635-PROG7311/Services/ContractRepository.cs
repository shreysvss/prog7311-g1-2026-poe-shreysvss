//Code attribution
//Title: Repository Pattern with ASP.NET Core and Entity Framework
//Author: Code Maze
//Date: 15 April 2026
//Version: 1
//Availability: https://code-maze.com/the-repository-pattern-aspnet-core

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;

namespace shrey_st10438635_PROG7311.Services
{
    // The interface that defines what a contract repository must be able to do
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

    // Handles all database access for contracts so controllers never talk to the database directly
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Return every contract with its client loaded, newest first
        public async Task<List<Contract>> GetAllAsync()
            => await _context.Contracts.Include(c => c.Client).OrderByDescending(c => c.CreatedAt).ToListAsync();

        // Find a single contract by ID and load its client and service requests too
        public async Task<Contract?> GetByIdAsync(int id)
            => await _context.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).FirstOrDefaultAsync(c => c.Id == id);

        // Filter contracts by date range and/or status using LINQ
        // Only the filters the user actually provides get added to the query
        public async Task<List<Contract>> FilterAsync(DateTime? startFrom, DateTime? startTo, ContractStatus? status)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (startFrom.HasValue)
                query = query.Where(c => c.StartDate >= startFrom.Value);

            if (startTo.HasValue)
                query = query.Where(c => c.StartDate <= startTo.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        // Return only the contracts that are currently Active
        public async Task<List<Contract>> GetActiveContractsAsync()
            => await _context.Contracts.Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

        // Add a new contract to the database
        public async Task AddAsync(Contract contract) => await _context.Contracts.AddAsync(contract);

        // Update an existing contract in the database
        public async Task UpdateAsync(Contract contract) => _context.Contracts.Update(contract);

        // Delete a contract by ID if it exists
        public async Task DeleteAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null) _context.Contracts.Remove(contract);
        }

        // Commit all pending changes to the database
        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}