using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SportCourtManagent_Server.Migrations
{
    /// <inheritdoc />
    public partial class MergeManagerStaffCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffShifts_StaffId",
                table: "StaffShifts");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ShiftDate",
                table: "StaffShifts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "ComplexId",
                table: "StaffShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "StaffShifts",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Services",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Services",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Promotions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Promotions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Promotions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscount",
                table: "Promotions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderAmount",
                table: "Promotions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UsageLimit",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsedCount",
                table: "Promotions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComplexCourtTypeServices",
                columns: table => new
                {
                    OfferingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplexId = table.Column<int>(type: "int", nullable: false),
                    CourtTypeId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQty = table.Column<int>(type: "int", nullable: false),
                    ServiceMode = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexCourtTypeServices", x => x.OfferingId);
                    table.ForeignKey(
                        name: "FK_ComplexCourtTypeServices_CourtComplexes_ComplexId",
                        column: x => x.ComplexId,
                        principalTable: "CourtComplexes",
                        principalColumn: "ComplexId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplexCourtTypeServices_CourtTypes_CourtTypeId",
                        column: x => x.CourtTypeId,
                        principalTable: "CourtTypes",
                        principalColumn: "CourtTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplexCourtTypeServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    TournamentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.TournamentId);
                    table.ForeignKey(
                        name: "FK_Tournaments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CourtTypes",
                columns: new[] { "CourtTypeId", "IsActive", "TypeName" },
                values: new object[,]
                {
                    { 1, true, "Pickleball" },
                    { 2, true, "Badminton" },
                    { 3, true, "Football" }
                });

            migrationBuilder.InsertData(
                table: "MembershipTiers",
                columns: new[] { "TierId", "DiscountPercent", "MinPoints", "TierName" },
                values: new object[,]
                {
                    { 1, 0.00m, 0, "Bronze" },
                    { 2, 5.00m, 100, "Silver" },
                    { 3, 10.00m, 500, "Gold" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, "System Administrator", "Admin" },
                    { 2, "Complex Manager", "Manager" },
                    { 3, "Staff member", "Staff" },
                    { 4, "End Customer", "Customer" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "ServiceId", "Category", "CreatedAt", "Description", "IsActive", "Price", "ServiceName", "StockQty", "Unit" },
                values: new object[,]
                {
                    { 1, "EquipmentRent", new DateTime(2026, 7, 5, 14, 54, 31, 110, DateTimeKind.Utc).AddTicks(7482), null, true, 30000.00m, "Thuê vợt Pickleball", 20, "cái" },
                    { 2, "EquipmentRent", new DateTime(2026, 7, 5, 14, 54, 31, 110, DateTimeKind.Utc).AddTicks(7489), null, true, 20000.00m, "Thuê vợt cầu lông", 30, "cái" },
                    { 3, "Drink", new DateTime(2026, 7, 5, 14, 54, 31, 110, DateTimeKind.Utc).AddTicks(7491), null, true, 15000.00m, "Nước uống Pocari", 100, "cái" },
                    { 4, "Drink", new DateTime(2026, 7, 5, 14, 54, 31, 110, DateTimeKind.Utc).AddTicks(7493), null, true, 10000.00m, "Nước suối Aquafina", 150, "cái" }
                });

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "SlotId", "DayType", "EndTime", "SlotName", "StartTime" },
                values: new object[,]
                {
                    { 1, 0, new TimeSpan(0, 7, 30, 0, 0), "Slot 1 (06:00 - 07:30)", new TimeSpan(0, 6, 0, 0, 0) },
                    { 2, 0, new TimeSpan(0, 9, 0, 0, 0), "Slot 2 (07:30 - 09:00)", new TimeSpan(0, 7, 30, 0, 0) },
                    { 3, 0, new TimeSpan(0, 10, 30, 0, 0), "Slot 3 (09:00 - 10:30)", new TimeSpan(0, 9, 0, 0, 0) },
                    { 4, 0, new TimeSpan(0, 16, 30, 0, 0), "Slot 4 (15:00 - 16:30)", new TimeSpan(0, 15, 0, 0, 0) },
                    { 5, 0, new TimeSpan(0, 18, 0, 0, 0), "Slot 5 (16:30 - 18:00)", new TimeSpan(0, 16, 30, 0, 0) },
                    { 6, 0, new TimeSpan(0, 19, 30, 0, 0), "Slot 6 (18:00 - 19:30)", new TimeSpan(0, 18, 0, 0, 0) },
                    { 7, 0, new TimeSpan(0, 21, 0, 0, 0), "Slot 7 (19:30 - 21:00)", new TimeSpan(0, 19, 30, 0, 0) },
                    { 8, 0, new TimeSpan(0, 22, 30, 0, 0), "Slot 8 (21:00 - 22:30)", new TimeSpan(0, 21, 0, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AvatarUrl", "CreatedAt", "DateOfBirth", "Email", "FullName", "Gender", "IsActive", "LoyaltyPoints", "MembershipTierId", "PasswordHash", "Phone", "RefreshToken", "SkillLevel" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@sportcourt.com", "System Administrator", 2, true, 0, null, "$2a$11$qR3gWwH8wF6hKqU6sXn9O.H2QJ1WJ5tQ.z5eJjU5tK8l8tS8z8z8z", "0987654321", null, 2 },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "manager@sportcourt.com", "Complex Manager", 0, true, 0, null, "$2a$11$qR3gWwH8wF6hKqU6sXn9O.H2QJ1WJ5tQ.z5eJjU5tK8l8tS8z8z8z", "0987654322", null, 1 },
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "staff@sportcourt.com", "Staff Member", 1, true, 0, null, "$2a$11$qR3gWwH8wF6hKqU6sXn9O.H2QJ1WJ5tQ.z5eJjU5tK8l8tS8z8z8z", "0987654323", null, 0 }
                });

            migrationBuilder.InsertData(
                table: "CourtComplexes",
                columns: new[] { "ComplexId", "Address", "ComplexName", "CreatedAt", "Description", "ImageUrl", "IsDeleted", "ManagerId" },
                values: new object[] { 1, "Dịch Vọng, Cầu Giấy, Hà Nội", "Tổ hợp thể thao Cầu Giấy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tổ hợp thể thao hiện đại bậc nhất khu vực Cầu Giấy với nhiều loại sân khác nhau.", "https://example.com/complex1.jpg", false, 2 });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserRoleId", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AvatarUrl", "CreatedAt", "DateOfBirth", "Email", "FullName", "Gender", "IsActive", "LoyaltyPoints", "MembershipTierId", "PasswordHash", "Phone", "RefreshToken", "SkillLevel" },
                values: new object[] { 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer@sportcourt.com", "John Doe", 2, true, 50, 1, "$2a$11$qR3gWwH8wF6hKqU6sXn9O.H2QJ1WJ5tQ.z5eJjU5tK8l8tS8z8z8z", "0987654324", null, 0 });

            migrationBuilder.InsertData(
                table: "Courts",
                columns: new[] { "CourtId", "CloseTime", "ComplexId", "CourtCode", "CourtName", "CourtSize", "CourtTypeId", "IsDeleted", "OpenTime", "PricePerHour", "Status" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 22, 0, 0, 0), 1, "PB-P1", "Sân Pickleball P1", "20x44 feet", 1, false, new TimeSpan(0, 6, 0, 0, 0), 150000.00m, 0 },
                    { 2, new TimeSpan(0, 22, 0, 0, 0), 1, "BM-B1", "Sân Cầu Lông B1", "6.1x13.4 meters", 2, false, new TimeSpan(0, 6, 0, 0, 0), 100000.00m, 0 },
                    { 3, new TimeSpan(0, 22, 0, 0, 0), 1, "FB-F1", "Sân Bóng Đá F1", "5-a-side", 3, false, new TimeSpan(0, 6, 0, 0, 0), 300000.00m, 0 }
                });

            migrationBuilder.InsertData(
                table: "StaffComplexes",
                columns: new[] { "StaffComplexId", "AssignedAt", "ComplexId", "StaffId" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 3 });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserRoleId", "RoleId", "UserId" },
                values: new object[] { 4, 4, 4 });

            migrationBuilder.InsertData(
                table: "CourtPricing",
                columns: new[] { "PricingId", "CourtId", "EffectiveFrom", "Price", "SlotId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 120000.00m, 1 },
                    { 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 120000.00m, 2 },
                    { 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 120000.00m, 3 },
                    { 4, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 150000.00m, 4 },
                    { 5, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 150000.00m, 5 },
                    { 6, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 180000.00m, 6 },
                    { 7, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 180000.00m, 7 },
                    { 8, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 180000.00m, 8 },
                    { 9, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 80000.00m, 1 },
                    { 10, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 80000.00m, 2 },
                    { 11, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 80000.00m, 3 },
                    { 12, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 100000.00m, 4 },
                    { 13, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 100000.00m, 5 },
                    { 14, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 120000.00m, 6 },
                    { 15, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 120000.00m, 7 },
                    { 16, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 120000.00m, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_ComplexId_ShiftDate",
                table: "StaffShifts",
                columns: new[] { "ComplexId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_StaffId_ShiftDate_ShiftType",
                table: "StaffShifts",
                columns: new[] { "StaffId", "ShiftDate", "ShiftType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TournamentId",
                table: "Bookings",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplexCourtTypeServices_ComplexId_CourtTypeId_ServiceId",
                table: "ComplexCourtTypeServices",
                columns: new[] { "ComplexId", "CourtTypeId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplexCourtTypeServices_CourtTypeId",
                table: "ComplexCourtTypeServices",
                column: "CourtTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplexCourtTypeServices_ServiceId",
                table: "ComplexCourtTypeServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplexes_ComplexId",
                table: "StaffComplexes",
                column: "ComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplexes_StaffId_ComplexId",
                table: "StaffComplexes",
                columns: new[] { "StaffId", "ComplexId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_UserId",
                table: "Tournaments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Tournaments_TournamentId",
                table: "Bookings",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Bookings_Tournaments_TournamentId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffShifts_CourtComplexes_ComplexId",
                table: "StaffShifts");

            migrationBuilder.DropTable(
                name: "ComplexCourtTypeServices");

            migrationBuilder.DropTable(
                name: "StaffComplexes");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_StaffShifts_ComplexId_ShiftDate",
                table: "StaffShifts");

            migrationBuilder.DropIndex(
                name: "IX_StaffShifts_StaffId_ShiftDate_ShiftType",
                table: "StaffShifts");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TournamentId",
                table: "Bookings");

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "CourtPricing",
                keyColumn: "PricingId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "CourtId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MembershipTiers",
                keyColumn: "TierId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MembershipTiers",
                keyColumn: "TierId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "ServiceId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "ServiceId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "ServiceId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "ServiceId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CourtTypes",
                keyColumn: "CourtTypeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "CourtId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "CourtId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CourtComplexes",
                keyColumn: "ComplexId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CourtTypes",
                keyColumn: "CourtTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CourtTypes",
                keyColumn: "CourtTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MembershipTiers",
                keyColumn: "TierId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "ComplexId",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxDiscount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MinOrderAmount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "UsageLimit",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "UsedCount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Bookings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShiftDate",
                table: "StaffShifts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_StaffId",
                table: "StaffShifts",
                column: "StaffId");
        }
    }
}
