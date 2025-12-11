using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutoringApp.Migrations
{
    /// <inheritdoc />
    public partial class CreateNumberMec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumberMec",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberMec",
                table: "Users");
        }
    }
}
