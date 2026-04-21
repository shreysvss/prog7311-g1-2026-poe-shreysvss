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
    // The possible statuses a contract can be in
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    // The different service levels a contract can have
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

        // Path to the uploaded PDF agreement on the server
        [Display(Name = "Signed Agreement (PDF)")]
        public string? SignedAgreementPath { get; set; }

        // Keeps the original file name the user uploaded, for display purposes
        [Display(Name = "Original File Name")]
        public string? SignedAgreementFileName { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Every contract belongs to one client
        public Client? Client { get; set; }

        // A contract can have many service requests linked to it
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}