using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;
using shrey_st10438635_PROG7311.Services;

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
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkflowService _workflowService;

        public ServiceRequestsController(ApplicationDbContext context,
                                          ICurrencyService currencyService,
                                          IWorkflowService workflowService)
        {
            _context = context;
            _currencyService = currencyService;
            _workflowService = workflowService;
        }

        // GET: ServiceRequests  (W3Schools, 2026)
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(sr => sr.RequestedOn)
                .ToListAsync();
            return View(requests);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var request = await _context.ServiceRequests
                .Include(sr => sr.Contract).ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(sr => sr.Id == id);
            if (request == null) return NotFound();
            return View(request);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create(int? contractId)
        {
            // Default to USD on page load
            var rate = await _currencyService.GetRateToZarAsync("USD");

            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            var vm = new ServiceRequestCreateViewModel
            {
                ActiveContracts = activeContracts,
                SupportedCurrencies = _currencyService.SupportedCurrencies.ToList(),
                SourceCurrency = "USD",
                ExchangeRate = rate,
                ContractId = contractId ?? 0
            };

            ViewBag.ExchangeRate = rate;
            return View(vm);
        }

        // POST: ServiceRequests/Create  (Tutorials Teacher, 2026)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestCreateViewModel vm)
        {
            // Remove auto-calculated fields from validation — they are set by JavaScript
            ModelState.Remove("ExchangeRate");
            ModelState.Remove("EstimatedZAR");

            // Repopulate dropdown data in case we return the view
            vm.ActiveContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();
            vm.SupportedCurrencies = _currencyService.SupportedCurrencies.ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.ExchangeRate = vm.ExchangeRate;
                return View(vm);
            }

            // Workflow Validation
            var contract = await _context.Contracts.FindAsync(vm.ContractId);
            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Selected contract does not exist.");
                return View(vm);
            }

            var (canCreate, errorMsg) = _workflowService.CanCreateServiceRequest(contract);
            if (!canCreate)
            {
                ModelState.AddModelError(string.Empty, errorMsg);
                ViewBag.ExchangeRate = vm.ExchangeRate;
                return View(vm);
            }

            // Currency Conversion (any supported source currency to ZAR)
            var rate = await _currencyService.GetRateToZarAsync(vm.SourceCurrency);
            var zarAmount = _currencyService.ConvertToZar(vm.CostAmount, rate);

            var serviceRequest = new ServiceRequest
            {
                ContractId = vm.ContractId,
                Description = vm.Description,
                SourceCurrency = vm.SourceCurrency.ToUpperInvariant(),
                Cost = vm.CostAmount,   // stores the original entered amount
                CostZAR = zarAmount,
                ExchangeRateUsed = rate,
                Status = ServiceRequestStatus.Pending,
                RequestedOn = DateTime.Now
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Service Request created. {vm.SourceCurrency} {vm.CostAmount:F2} = ZAR {zarAmount:F2} (rate: {rate:F4}).";
            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sr = await _context.ServiceRequests.Include(s => s.Contract).FirstOrDefaultAsync(s => s.Id == id);
            if (sr == null) return NotFound();
            ViewBag.Contracts = new SelectList(_context.Contracts.ToList(), "Id", "Title", sr.ContractId);
            return View(sr);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ContractId,Description,SourceCurrency,Cost,CostZAR,ExchangeRateUsed,Status,RequestedOn")] ServiceRequest sr)
        {
            if (id != sr.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(sr);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service Request updated.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Contracts = new SelectList(_context.Contracts.ToList(), "Id", "Title", sr.ContractId);
            return View(sr);
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sr = await _context.ServiceRequests.Include(s => s.Contract).ThenInclude(c => c!.Client).FirstOrDefaultAsync(s => s.Id == id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr != null)
            {
                _context.ServiceRequests.Remove(sr);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service Request deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: GET /ServiceRequests/GetRate?currency=EUR — returns current rate to ZAR as JSON
        [HttpGet]
        public async Task<IActionResult> GetRate(string currency = "USD")
        {
            try
            {
                var rate = await _currencyService.GetRateToZarAsync(currency);
                return Json(new { rate, currency = currency.ToUpperInvariant() });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { error = $"Currency '{currency}' not supported." });
            }
        }
    }
}