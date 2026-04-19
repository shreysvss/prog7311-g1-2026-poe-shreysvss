using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace shrey_st10438635_PROG7311.Models

// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <Microsoft, 2026. Entity Framework Core Overview. [online] Microsoft Learn. Available at: https://learn.microsoft.com/en-us/ef/core [Accessed 15 April 2026].>
// <Entity Framework Tutorial, 2026. Code-First Approach in EF Core. [online] Available at: https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx [Accessed 16 April 2026].>
// <W3Schools, 2026. SQL FOREIGN KEY Constraint. [online] W3Schools. Available at: https://www.w3schools.com/sql/sql_foreignkey.asp [Accessed 17 April 2026].>
// <TutorialsTeacher, 2026. Data Annotations in EF Core. [online] Available at: https://www.tutorialsteacher.com/efcore/fluent-api-vs-data-annotation-attributes [Accessed 18 April 2026].>
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
        [Display(Name = "Source Currency")]
        [StringLength(3)]
        public string SourceCurrency { get; set; } = "USD";
        [Required]
        [Display(Name = "Cost")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
        [Column("Cost", TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
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