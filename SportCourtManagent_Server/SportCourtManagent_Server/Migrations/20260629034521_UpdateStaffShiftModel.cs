using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportCourtManagent_Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStaffShiftModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "StaffShifts",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "StaffShifts");
        }
    }
}
