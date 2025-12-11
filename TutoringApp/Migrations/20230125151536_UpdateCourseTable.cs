using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutoringApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseCode",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE Courses SET CourseCode = CourseId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Courses_CourseCode",
                table: "Courses",
                column: "CourseCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseCode",
                table: "Courses");
        }
    }
}
