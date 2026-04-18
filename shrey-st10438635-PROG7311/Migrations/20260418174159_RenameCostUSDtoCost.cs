using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace shrey_st10438635_PROG7311.Migrations
{
    /// <inheritdoc />
    public partial class RenameCostUSDtoCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CostUSD",
                table: "ServiceRequests",
                newName: "Cost");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "ServiceRequests",
                newName: "CostUSD");
        }
    }
}
