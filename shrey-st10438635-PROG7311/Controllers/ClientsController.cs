//Code attribution
//Title: Get started with ASP.NET Core MVC
//Author: Microsoft
//Date: 15 April 2026
//Version: 1
//Availability: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc

//Code attribution
//Title: ASP.NET Core MVC Controllers
//Author: Tutorials Teacher
//Date: 17 April 2026
//Version: 1
//Availability: https://www.tutorialsteacher.com/core/aspnet-core-mvc-controller

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;

namespace shrey_st10438635_PROG7311.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show a list of all clients along with their linked contracts
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.Include(c => c.Contracts).ToListAsync();
            return View(clients);
        }

        // Show the details of a single client by ID
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // Show the form for creating a new client
        public IActionResult Create() => View();

        // Save a new client to the database when the form is submitted
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

        // Show the form for editing an existing client
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // Save the updated client to the database when the form is submitted
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

        // Show the delete confirmation page for a client
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // Delete the client from the database once the delete is confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Load the client along with their contracts so we can check before deleting
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                TempData["Error"] = "Client not found.";
                return RedirectToAction(nameof(Index));
            }

            // Block the delete if the client still has contracts attached to them
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
                // Safety net just in case a contract gets added between the check and the save
                TempData["Error"] = $"Cannot delete '{client.Name}' because of existing related records in the database.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}