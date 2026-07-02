using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportCourtManagerment.Migrations
{
    /// <inheritdoc />
    public partial class AddCourtComplexAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComplexId",
                table: "Courts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourtSize",
                table: "Courts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Courts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "Courts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CourtComplexes",
                columns: table => new
                {
                    ComplexId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplexName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtComplexes", x => x.ComplexId);
                    table.ForeignKey(
                        name: "FK_CourtComplexes_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courts_ComplexId",
                table: "Courts",
                column: "ComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_IsDeleted",
                table: "Courts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CourtComplexes_IsDeleted",
                table: "CourtComplexes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CourtComplexes_ManagerId",
                table: "CourtComplexes",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_CourtComplexes_ComplexId",
                table: "Courts",
                column: "ComplexId",
                principalTable: "CourtComplexes",
                principalColumn: "ComplexId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courts_CourtComplexes_ComplexId",
                table: "Courts");

            migrationBuilder.DropTable(
                name: "CourtComplexes");

            migrationBuilder.DropIndex(
                name: "IX_Courts_ComplexId",
                table: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_Courts_IsDeleted",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "ComplexId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "CourtSize",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "Courts");
        }
    }
}
