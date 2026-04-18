using System.ComponentModel.DataAnnotations;
namespace shrey_st10438635_PROG7311.Models
{
    // Used for Contract filter/search
    public class ContractFilterViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Start Date From")]
        public DateTime? StartDateFrom { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Start Date To")]
        public DateTime? StartDateTo { get; set; }
        public ContractStatus? Status { get; set; }
        public List<Contract> Contracts { get; set; } = new List<Contract>();
    }
    // Used for ServiceRequest creation — includes currency info
    public class ServiceRequestCreateViewModel
    {
        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Source Currency")]
        public string SourceCurrency { get; set; } = "USD";
        [Required]
        [Display(Name = "Cost")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be a positive value.")]
        public decimal CostAmount { get; set; }
        // Filled in via JS from API call, then confirmed on submit
        [Display(Name = "Exchange Rate (to ZAR)")]
        public decimal ExchangeRate { get; set; }
        [Display(Name = "Estimated Cost (ZAR)")]
        public decimal EstimatedZAR { get; set; }
        // For dropdowns
        public List<Contract>? ActiveContracts { get; set; }
        public List<string> SupportedCurrencies { get; set; } = new List<string>();
    }
}