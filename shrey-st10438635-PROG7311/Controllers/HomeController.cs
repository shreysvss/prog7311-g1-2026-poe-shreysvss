using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;

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
