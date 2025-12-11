using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutoringApp.Migrations
{
    /// <inheritdoc />
    public partial class AddYearIdtogroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YearId",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YearId",
                table: "Groups");
        }
    }
}
