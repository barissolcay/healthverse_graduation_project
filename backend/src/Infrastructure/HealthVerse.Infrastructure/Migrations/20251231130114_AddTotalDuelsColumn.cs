using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalDuelsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalDuels",
                schema: "identity",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDuels",
                schema: "identity",
                table: "Users");
        }
    }
}
