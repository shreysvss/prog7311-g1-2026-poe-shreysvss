using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;

namespace shrey_st10438635_PROG7311.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalClients = await _context.Clients.CountAsync();
            ViewBag.TotalContracts = await _context.Contracts.CountAsync();
            ViewBag.ActiveContracts = await _context.Contracts.CountAsync(c => c.Status == Models.ContractStatus.Active);
            ViewBag.TotalRequests = await _context.ServiceRequests.CountAsync();
            return View();
        }
    }
}
