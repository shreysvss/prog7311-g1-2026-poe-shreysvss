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

        // Navigation
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
