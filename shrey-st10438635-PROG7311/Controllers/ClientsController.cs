using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;


// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <Microsoft, 2026. Overview of ASP.NET Core MVC. [online] Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/overview [Accessed 15 April 2026].>
// <W3Schools, 2026. ASP.NET MVC Tutorial. [online] W3Schools. Available at: https://www.w3schools.com/asp/mvc_intro.asp [Accessed 16 April 2026].>
// <Tutorials Teacher, 2026. Model Binding in ASP.NET Core MVC. [online] Available at: https://www.tutorialsteacher.com/core/aspnet-core-mvc-model-binding [Accessed 17 April 2026].>
// <Code Maze, 2026. File Upload in ASP.NET Core MVC. [online] Available at: https://code-maze.com/file-upload-aspnetcore-mvc [Accessed 18 April 2026].>



namespace shrey_st10438635_PROG7311.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.Include(c => c.Contracts).ToListAsync();
            return View(clients);
        }
        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }
        // GET: Clients/Create
        public IActionResult Create() => View();
        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ContactEmail,ContactPhone,Region")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Client '{client.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }
        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }
        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactEmail,ContactPhone,Region")] Client client)
        {
            if (id != client.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Client '{client.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Clients.Any(c => c.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }
        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }
        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Load client with contracts so we can check for dependencies before deleting
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                TempData["Error"] = "Client not found.";
                return RedirectToAction(nameof(Index));
            }

            // Block deletion if the client has linked contracts — referential integrity protection
            if (client.Contracts != null && client.Contracts.Any())
            {
                var count = client.Contracts.Count;
                TempData["Error"] = $"Cannot delete '{client.Name}' because they have {count} linked contract(s). Please delete or reassign the contract(s) first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Client '{client.Name}' deleted successfully.";
            }
            catch (DbUpdateException)
            {
                // Safety net — in case a contract slips in between the check and the save
                TempData["Error"] = $"Cannot delete '{client.Name}' because of existing related records in the database.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}