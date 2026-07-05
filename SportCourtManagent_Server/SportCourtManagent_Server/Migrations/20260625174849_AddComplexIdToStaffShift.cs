using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportCourtManagent_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddComplexIdToStaffShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffShifts_StaffId",
                table: "StaffShifts");

            migrationBuilder.AddColumn<int>(
                name: "ComplexId",
                table: "StaffShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_ComplexId_ShiftDate",
                table: "StaffShifts",
                columns: new[] { "ComplexId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_StaffId_ShiftDate_ShiftType",
                table: "StaffShifts",
                columns: new[] { "StaffId", "ShiftDate", "ShiftType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffShifts_CourtComplexes_ComplexId",
                table: "StaffShifts",
                column: "ComplexId",
                principalTable: "CourtComplexes",
                principalColumn: "ComplexId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffShifts_CourtComplexes_ComplexId",
                table: "StaffShifts");

            migrationBuilder.DropIndex(
                name: "IX_StaffShifts_ComplexId_ShiftDate",
                table: "StaffShifts");

            migrationBuilder.DropIndex(
                name: "IX_StaffShifts_StaffId_ShiftDate_ShiftType",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "ComplexId",
                table: "StaffShifts");

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_StaffId",
                table: "StaffShifts",
                column: "StaffId");
        }
    }
}
