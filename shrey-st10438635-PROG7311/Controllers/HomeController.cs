//Code attribution
//Title: Get started with ASP.NET Core MVC
//Author: Microsoft
//Date: 15 April 2026
//Version: 1
//Availability: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc

//Code attribution
//Title: ASP.NET MVC Tutorial
//Author: W3Schools
//Date: 16 April 2026
//Version: 1
//Availability: https://www.w3schools.com/asp/mvc_intro.asp

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


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

        // Show the dashboard with summary counts for clients, contracts, and service requests
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