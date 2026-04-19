using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <Microsoft, 2026. Entity Framework Core Overview. [online] Microsoft Learn. Available at: https://learn.microsoft.com/en-us/ef/core [Accessed 15 April 2026].>
// <Entity Framework Tutorial, 2026. Code-First Approach in EF Core. [online] Available at: https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx [Accessed 16 April 2026].>
// <W3Schools, 2026. SQL FOREIGN KEY Constraint. [online] W3Schools. Available at: https://www.w3schools.com/sql/sql_foreignkey.asp [Accessed 17 April 2026].>
// <TutorialsTeacher, 2026. Data Annotations in EF Core. [online] Available at: https://www.tutorialsteacher.com/efcore/fluent-api-vs-data-annotation-attributes [Accessed 18 April 2026].>

namespace shrey_st10438635_PROG7311.Models
{
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public enum ServiceLevel
    {
        Standard,
        Express,
        Premium
    }

    public class Contract
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Contract title is required.")]
        [StringLength(200)]
        [Display(Name = "Contract Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        [Display(Name = "Service Level")]
        public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

        // PDF file path stored on disk
        [Display(Name = "Signed Agreement (PDF)")]
        public string? SignedAgreementPath { get; set; }

        [Display(Name = "Original File Name")]
        public string? SignedAgreementFileName { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Client? Client { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
