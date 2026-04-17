using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shrey_st10438635_PROG7311.Models
{
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

        [Required]
        [Display(Name = "Cost (USD)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        [Display(Name = "Cost (ZAR)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        [Display(Name = "Exchange Rate Used")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }

        [Required]
        [Display(Name = "Status")]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [DataType(DataType.DateTime)]
        [Display(Name = "Requested On")]
        public DateTime RequestedOn { get; set; } = DateTime.Now;

        // Navigation
        public Contract? Contract { get; set; }
    }
}
