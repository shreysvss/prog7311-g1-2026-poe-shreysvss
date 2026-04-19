using System.ComponentModel.DataAnnotations;

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
