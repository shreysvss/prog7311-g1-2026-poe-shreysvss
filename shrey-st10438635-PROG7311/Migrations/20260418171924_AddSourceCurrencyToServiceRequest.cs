using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace shrey_st10438635_PROG7311.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceCurrencyToServiceRequest : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceCurrency",
                table: "ServiceRequests",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceCurrency",
                table: "ServiceRequests");
        }
    }
}
