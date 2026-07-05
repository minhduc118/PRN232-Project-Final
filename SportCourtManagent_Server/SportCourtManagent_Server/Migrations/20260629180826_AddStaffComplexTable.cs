using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportCourtManagent_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffComplexTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffComplexes",
                columns: table => new
                {
                    StaffComplexId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ComplexId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffComplexes", x => x.StaffComplexId);
                    table.ForeignKey(
                        name: "FK_StaffComplexes_CourtComplexes_ComplexId",
                        column: x => x.ComplexId,
                        principalTable: "CourtComplexes",
                        principalColumn: "ComplexId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffComplexes_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "StaffComplexes",
                columns: new[] { "StaffComplexId", "AssignedAt", "ComplexId", "StaffId" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 3 });

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplexes_ComplexId",
                table: "StaffComplexes",
                column: "ComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplexes_StaffId_ComplexId",
                table: "StaffComplexes",
                columns: new[] { "StaffId", "ComplexId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffComplexes");
        }
    }
}
