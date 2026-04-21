//Code attribution
//Title: Code-First Approach in EF Core
//Author: Entity Framework Tutorial
//Date: 16 April 2026
//Version: 1
//Availability: https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shrey_st10438635_PROG7311.Models
{
    // The possible statuses a service request can be in
    public enum ServiceRequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // The currency the user entered the cost in (USD, EUR, GBP, etc.)
        [Required]
        [Display(Name = "Source Currency")]
        [StringLength(3)]
        public string SourceCurrency { get; set; } = "USD";

        // The original cost amount in whatever source currency was chosen
        [Required]
        [Display(Name = "Cost")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
        [Column("Cost", TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        // The same cost, but converted to ZAR using the live exchange rate
        [Display(Name = "Cost (ZAR)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        // Stored so we have a record of what the rate was when this was created
        [Display(Name = "Exchange Rate Used")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }

        [Required]
        [Display(Name = "Status")]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [DataType(DataType.DateTime)]
        [Display(Name = "Requested On")]
        public DateTime RequestedOn { get; set; } = DateTime.Now;

        // Every service request belongs to one contract
        public Contract? Contract { get; set; }
    }
}