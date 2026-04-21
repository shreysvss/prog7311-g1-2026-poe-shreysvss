//Code attribution
//Title: Upload files in ASP.NET Core
//Author: Microsoft
//Date: 15 April 2026
//Version: 1
//Availability: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads

//Code attribution
//Title: File Upload in ASP.NET Core MVC
//Author: Code Maze
//Date: 18 April 2026
//Version: 1
//Availability: https://code-maze.com/file-upload-aspnetcore-mvc

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using shrey_st10438635_PROG7311.Data;
using shrey_st10438635_PROG7311.Models;
using shrey_st10438635_PROG7311.Services;

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

        // Show the list of all contracts, with optional filters for date range and status
        public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? startDateTo, ContractStatus? status)
        {
            List<Contract> contracts;

            // If any filter is set, run the filtered query, otherwise get them all
            if (startDateFrom.HasValue || startDateTo.HasValue || status.HasValue)
                contracts = await _contractRepo.FilterAsync(startDateFrom, startDateTo, status);
            else
                contracts = await _contractRepo.GetAllAsync();

            // Move any Active contracts past their end date into Expired status
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

        // Show the details of a single contract by ID
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null) return NotFound();
            return View(contract);
        }

        // Show the form for creating a new contract
        public IActionResult Create()
        {
            ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name");
            return View();
        }

        // Save a new contract to the database when the form is submitted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Title,StartDate,EndDate,Status,ServiceLevel")] Contract contract,
                                                 IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                // Handle the signed agreement PDF upload if one was provided
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    // Check that the file is actually a PDF
                    if (!_fileService.IsValidPdf(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", "Only PDF files are allowed for the Signed Agreement.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }

                    // Check that the file is not larger than the size limit
                    if (!_fileService.IsWithinSizeLimit(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", $"The Signed Agreement must be {FileService.MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }

                    // Save the file to disk and store the path in the contract
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

        // Show the form for editing an existing contract
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null) return NotFound();
            ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // Save the updated contract to the database when the form is submitted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,Title,StartDate,EndDate,Status,ServiceLevel,SignedAgreementPath,SignedAgreementFileName,CreatedAt")] Contract contract,
                                               IFormFile? signedAgreement)
        {
            if (id != contract.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Handle a new PDF upload if the user picked one
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    // Check that the file is a PDF
                    if (!_fileService.IsValidPdf(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", "Only PDF files are allowed.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }

                    // Check that the file is within the size limit
                    if (!_fileService.IsWithinSizeLimit(signedAgreement))
                    {
                        ModelState.AddModelError("signedAgreement", $"The Signed Agreement must be {FileService.MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");
                        ViewBag.Clients = new SelectList(_context.Clients.ToList(), "Id", "Name", contract.ClientId);
                        return View(contract);
                    }

                    // Delete the old file before saving the new one
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

        // Show the delete confirmation page for a contract
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null) return NotFound();
            return View(contract);
        }

        // Delete the contract from the database once the delete is confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _contractRepo.GetByIdAsync(id);
            if (contract != null)
            {
                // Also delete the uploaded PDF from disk if one exists
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                    _fileService.DeleteFile(contract.SignedAgreementPath);

                await _contractRepo.DeleteAsync(id);
                await _contractRepo.SaveAsync();
                TempData["Success"] = "Contract deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Let the user download the signed agreement PDF for a contract
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _contractRepo.GetByIdAsync(id.Value);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();

            // Send the user to the file URL so the browser handles the download
            return Redirect(contract.SignedAgreementPath);
        }
    }
}