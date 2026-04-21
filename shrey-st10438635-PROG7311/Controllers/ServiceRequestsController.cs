//Code attribution
//Title: Consuming a Web API using HttpClient
//Author: Tutorials Teacher
//Date: 17 April 2026
//Version: 1
//Availability: https://www.tutorialsteacher.com/core/consume-web-api-httpclient

//Code attribution
//Title: Model Binding in ASP.NET Core MVC
//Author: Tutorials Teacher
//Date: 17 April 2026
//Version: 1
//Availability: https://www.tutorialsteacher.com/core/aspnet-core-mvc-model-binding

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;
using shrey_st10438635_PROG7311.Services;

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

        // Show the full list of service requests with newest first
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(sr => sr.RequestedOn)
                .ToListAsync();
            return View(requests);
        }

        // Show the details of a single service request by ID
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var request = await _context.ServiceRequests
                .Include(sr => sr.Contract).ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(sr => sr.Id == id);
            if (request == null) return NotFound();
            return View(request);
        }

        // Show the form for creating a new service request
        public async Task<IActionResult> Create(int? contractId)
        {
            // Load the page with USD as the default currency
            var rate = await _currencyService.GetRateToZarAsync("USD");

            // Only show active contracts in the dropdown
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

        // Save a new service request when the form is submitted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestCreateViewModel vm)
        {
            // These fields are filled in by JavaScript, so skip their server validation
            ModelState.Remove("ExchangeRate");
            ModelState.Remove("EstimatedZAR");

            // Reload the dropdown data in case we end up returning the form with errors
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

            // Check that the selected contract actually exists
            var contract = await _context.Contracts.FindAsync(vm.ContractId);
            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Selected contract does not exist.");
                return View(vm);
            }

            // Check the workflow rules to see if a service request can be raised on this contract
            var (canCreate, errorMsg) = _workflowService.CanCreateServiceRequest(contract);
            if (!canCreate)
            {
                ModelState.AddModelError(string.Empty, errorMsg);
                ViewBag.ExchangeRate = vm.ExchangeRate;
                return View(vm);
            }

            // Get the live rate for the chosen currency and convert the cost to ZAR
            var rate = await _currencyService.GetRateToZarAsync(vm.SourceCurrency);
            var zarAmount = _currencyService.ConvertToZar(vm.CostAmount, rate);

            var serviceRequest = new ServiceRequest
            {
                ContractId = vm.ContractId,
                Description = vm.Description,
                SourceCurrency = vm.SourceCurrency.ToUpperInvariant(),
                Cost = vm.CostAmount,
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

        // Show the form for editing an existing service request
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sr = await _context.ServiceRequests.Include(s => s.Contract).FirstOrDefaultAsync(s => s.Id == id);
            if (sr == null) return NotFound();
            ViewBag.Contracts = new SelectList(_context.Contracts.ToList(), "Id", "Title", sr.ContractId);
            return View(sr);
        }

        // Save the updated service request when the form is submitted
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

        // Show the delete confirmation page for a service request
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sr = await _context.ServiceRequests.Include(s => s.Contract).ThenInclude(c => c!.Client).FirstOrDefaultAsync(s => s.Id == id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        // Delete the service request from the database once confirmed
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

        // Returns the live exchange rate for a given currency as JSON
        // This is called from the browser by JavaScript when the user changes the currency dropdown
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