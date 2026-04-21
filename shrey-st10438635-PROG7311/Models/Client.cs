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
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(100)]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        [Display(Name = "Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required.")]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        // A client can have many contracts
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}