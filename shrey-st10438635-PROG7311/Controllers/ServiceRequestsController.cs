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

        // GET: ServiceRequests
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
            var rate = await _currencyService.GetUsdToZarRateAsync();
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            var vm = new ServiceRequestCreateViewModel
            {
                ActiveContracts = activeContracts,
                ExchangeRate = rate,
                ContractId = contractId ?? 0
            };

            ViewBag.ExchangeRate = rate;
            return View(vm);
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestCreateViewModel vm)
        {
            // Remove auto-calculated fields from validation — they are set by JavaScript
            ModelState.Remove("ExchangeRate");
            ModelState.Remove("EstimatedZAR");

            // Load active contracts for repopulating dropdown on error
            vm.ActiveContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.ExchangeRate = vm.ExchangeRate;
                return View(vm);
            }

            //Workflow Validation
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

            //Currency Conversion 
            var rate = await _currencyService.GetUsdToZarRateAsync();
            var zarAmount = _currencyService.ConvertUsdToZar(vm.CostUSD, rate);

            var serviceRequest = new ServiceRequest
            {
                ContractId = vm.ContractId,
                Description = vm.Description,
                CostUSD = vm.CostUSD,
                CostZAR = zarAmount,
                ExchangeRateUsed = rate,
                Status = ServiceRequestStatus.Pending,
                RequestedOn = DateTime.Now
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Service Request created. USD {vm.CostUSD:F2} = ZAR {zarAmount:F2} (rate: {rate:F4}).";
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,ContractId,Description,CostUSD,CostZAR,ExchangeRateUsed,Status,RequestedOn")] ServiceRequest sr)
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

        // AJAX: GET /ServiceRequests/GetRate — returns current USD→ZAR rate as JSON
        [HttpGet]
        public async Task<IActionResult> GetRate()
        {
            var rate = await _currencyService.GetUsdToZarRateAsync();
            return Json(new { rate });
        }
    }
}