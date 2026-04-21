//Code attribution
//Title: Data Annotations in EF Core
//Author: Tutorials Teacher
//Date: 18 April 2026
//Version: 1
//Availability: https://www.tutorialsteacher.com/efcore/fluent-api-vs-data-annotation-attributes

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


using System.ComponentModel.DataAnnotations;

namespace shrey_st10438635_PROG7311.Models
{
    // Used on the Contracts page for the search and filter form
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

    // Used on the Service Request create form, with all the fields needed for currency conversion
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

        // Filled in by JavaScript after fetching the live rate from the API
        [Display(Name = "Exchange Rate (to ZAR)")]
        public decimal ExchangeRate { get; set; }

        // Auto-calculated on the form as the user types the amount
        [Display(Name = "Estimated Cost (ZAR)")]
        public decimal EstimatedZAR { get; set; }

        // Used to populate the contract dropdown — only active contracts show up
        public List<Contract>? ActiveContracts { get; set; }

        // Used to populate the currency dropdown (USD, EUR, GBP, etc.)
        public List<string> SupportedCurrencies { get; set; } = new List<string>();
    }
}