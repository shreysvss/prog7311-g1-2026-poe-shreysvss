using Microsoft.EntityFrameworkCore;
using shrey_st10438635_PROG7311.Models;

// Shrey Singh
// ST10438635
// References:
// <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
// <Microsoft, 2026. Entity Framework Core Overview. [online] Microsoft Learn. Available at: https://learn.microsoft.com/en-us/ef/core [Accessed 15 April 2026].>
// <Entity Framework Tutorial, 2026. Code-First Approach in EF Core. [online] Available at: https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx [Accessed 16 April 2026].>
// <W3Schools, 2026. SQL FOREIGN KEY Constraint. [online] W3Schools. Available at: https://www.w3schools.com/sql/sql_foreignkey.asp [Accessed 17 April 2026].>
// <TutorialsTeacher, 2026. Data Annotations in EF Core. [online] Available at: https://www.tutorialsteacher.com/efcore/fluent-api-vs-data-annotation-attributes [Accessed 18 April 2026].>

namespace shrey_st10438635_PROG7311.Data //(W3Schools, 2026)
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Client → Contracts (One-to-Many)
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            // Contract → ServiceRequests (One-to-Many)
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            // Decimal precision using Fluent API
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.Cost)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.CostZAR)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.ExchangeRateUsed)
                .HasColumnType("decimal(18,4)");
            // Seed data
            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 1, Name = "Global Freight Co", ContactEmail = "freight@globalco.com", ContactPhone = "+27112345678", Region = "Africa" },
                new Client { Id = 2, Name = "EU Logistics Ltd", ContactEmail = "ops@eulogistics.eu", ContactPhone = "+441234567890", Region = "Europe" }
            );
            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    Id = 1,
                    ClientId = 1,
                    Title = "SA Distribution Contract",
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2026, 12, 31),
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLevel.Premium,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Contract
                {
                    Id = 2,
                    ClientId = 2,
                    Title = "EU Express Freight",
                    StartDate = new DateTime(2023, 6, 1),
                    EndDate = new DateTime(2024, 5, 31),
                    Status = ContractStatus.Expired,
                    ServiceLevel = ServiceLevel.Express,
                    CreatedAt = new DateTime(2023, 6, 1)
                }
            );
        }
    }
}