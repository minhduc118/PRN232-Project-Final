using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportCourtManagent_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddExpiredAtAndUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CourtId",
                table: "Bookings");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "Tournaments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Court_Slot_Date",
                table: "Bookings",
                columns: new[] { "CourtId", "SlotId", "BookingDate" },
                unique: true,
                filter: "[Status] != 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_Court_Slot_Date",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CourtId",
                table: "Bookings",
                column: "CourtId");
        }
    }
}
