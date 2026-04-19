using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class ContractsController : Controller
    {
        private readonly IContractRepository _contractRepo;
        private readonly IFileService _fileService;
        private readonly IWorkflowService _workflowService;
        private readonly ApplicationDbContext _context;

        public ContractsController(IContractRepository contractRepo,
                                   IFileService fileService,
                                   IWorkflowService workflowService,
                                   ApplicationDbContext context)
        {
            _contractRepo = contractRepo;
            _fileService = fileService;
            _workflowService = workflowService;
            _context = context;
        }

        // GET: Contracts (with optional search/filter)
        public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? startDateTo, ContractStatus? status)
        {
            List<Contract> contracts;

            if (startDateFrom.HasValue || startDateTo.HasValue || status.HasValue)
                contracts = await _contractRepo.FilterAsync(startDateFrom, startDateTo, status);
            else
                contracts = await _contractRepo.GetAllAsync();

            // Auto-expire contracts that have passed end date
            _workflowService.AutoExpireContracts(contracts);
            await _contractRepo.SaveAsync();

            var vm = new ContractFilterViewModel
            {
                StartDateFrom = startDateFrom,
                StartDateTo = startDateTo,
                Status = status,
                Contracts = contracts
            };

            return View(vm);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null) return NotFound();
            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Title,StartDate,EndDate,Status,ServiceLevel")] Contract contract,
                                                 IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                // Handle PDF upload
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    if (!_fileService.IsValidPdf(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", "Only PDF files are allowed for the Signed Agreement.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }
                    if (!_fileService.IsWithinSizeLimit(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", $"The Signed Agreement must be {FileService.MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }
                    var (path, fileName) = await _fileService.SaveContractFileAsync(signedAgreement);
                    contract.SignedAgreementPath = path;
                    contract.SignedAgreementFileName = fileName;
                }

                contract.CreatedAt = DateTime.Now;
                await _contractRepo.AddAsync(contract);
                await _contractRepo.SaveAsync();
                TempData["Success"] = $"Contract '{contract.Title}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null) return NotFound();
            ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,Title,StartDate,EndDate,Status,ServiceLevel,SignedAgreementPath,SignedAgreementFileName,CreatedAt")] Contract contract,
                                               IFormFile? signedAgreement)
        {
            if (id != contract.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    if (!_fileService.IsValidPdf(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", "Only PDF files are allowed.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }
                    if (!_fileService.IsWithinSizeLimit(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", $"The Signed Agreement must be {FileService.MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                        _fileService.DeleteFile(contract.SignedAgreementPath);

                    var (path, fileName) = await _fileService.SaveContractFileAsync(signedAgreement);
                    contract.SignedAgreementPath = path;
                    contract.SignedAgreementFileName = fileName;
                }

                await _contractRepo.UpdateAsync(contract);
                await _contractRepo.SaveAsync();
                TempData["Success"] = $"Contract '{contract.Title}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null) return NotFound();
            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _contractRepo.GetByIdAsync(id);
            if (contract != null)
            {
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                    _fileService.DeleteFile(contract.SignedAgreementPath);

                await _contractRepo.DeleteAsync(id);
                await _contractRepo.SaveAsync();
                TempData["Success"] = "Contract deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();

            // Serve file from wwwroot
            return Redirect(contract.SignedAgreementPath);
        }
    }
}