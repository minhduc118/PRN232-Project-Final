using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs.StaffDashboard;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/staff-dashboard")]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaffDashboardController(AppDbContext context)
        {
            _context = context;
        }

        // ── Legacy endpoint (kept for backward compatibility) ──
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            int customerRoleId = customerRole?.RoleId ?? 0;

            int totalCustomers = await _context.Users
                .Include(u => u.UserRoles)
                .CountAsync(u => u.UserRoles.Any(ur => ur.RoleId == customerRoleId));

            int totalEquipments = await _context.EquipmentInventories.CountAsync();

            var today = DateTime.Today;
            int todayBookings = await _context.Bookings
                .CountAsync(b => b.BookingDate.Date == today);

            int availableCourts = await _context.Courts
                .CountAsync(c => c.Status == CourtStatus.Available);

            var stats = new StaffStatsDto
            {
                TotalCustomers = totalCustomers,
                TotalEquipments = totalEquipments,
                TodayBookings = todayBookings,
                AvailableCourts = availableCourts
            };

            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Where(b => b.BookingDate.Date == today)
                .OrderByDescending(b => b.BookingId)
                .Take(5)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingCode,
                    CustomerName = b.User.FullName,
                    CourtName = b.Court.CourtName,
                    SlotName = b.TimeSlot.SlotName,
                    b.TotalAmount,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

            return Ok(new { data = stats, recentBookings });
        }

        // ── New comprehensive admin dashboard endpoint ──
        [HttpGet("admin-dashboard")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // ── Role IDs ──
            var customerRoleId = (await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer"))?.RoleId ?? 0;
            var coachRoleId    = (await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Coach"))?.RoleId ?? 0;

            // ── Revenue KPIs ──
            decimal todayRevenue = await _context.Bookings
                .Where(b => b.BookingDate.Date == today && b.Status != BookingStatus.Cancelled)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            decimal monthRevenue = await _context.Bookings
                .Where(b => b.BookingDate >= startOfMonth && b.Status != BookingStatus.Cancelled)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            // ── Booking KPIs ──
            int todayBookings   = await _context.Bookings.CountAsync(b => b.BookingDate.Date == today);
            int pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);

            // ── Court KPIs ──
            int totalCourts       = await _context.Courts.CountAsync(c => !c.IsDeleted);
            int availableCourts   = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Available);
            int maintenanceCourts = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Maintenance);
            int bookedCourts      = await _context.Courts.CountAsync(c => !c.IsDeleted && (c.Status == CourtStatus.Booked || c.Status == CourtStatus.InUse));
            int inactiveCourts    = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Inactive);

            double occupancyRate = totalCourts > 0
                ? Math.Round((double)bookedCourts / totalCourts * 100, 1) : 0;

            // ── User KPIs ──
            int activeCustomers = await _context.Users
                .Include(u => u.UserRoles)
                .CountAsync(u => u.IsActive && u.UserRoles.Any(ur => ur.RoleId == customerRoleId));

            int activeCoaches = await _context.Users
                .Include(u => u.UserRoles)
                .CountAsync(u => u.IsActive && u.UserRoles.Any(ur => ur.RoleId == coachRoleId));

            // ── Service KPIs ──
            int lowStockServices = await _context.Services.CountAsync(s => s.IsActive && s.StockQty < 10);

            // ── Revenue Chart: last 7 days ──
            var revenueChart = new List<object>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var rev  = await _context.Bookings
                    .Where(b => b.BookingDate.Date == date && b.Status != BookingStatus.Cancelled)
                    .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;
                var cnt  = await _context.Bookings.CountAsync(b => b.BookingDate.Date == date);
                revenueChart.Add(new { date = date.ToString("dd/MM"), revenue = rev, bookings = cnt });
            }

            // ── Recent Bookings: 8 newest ──
            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court).ThenInclude(c => c.CourtType)
                .Include(b => b.TimeSlot)
                .OrderByDescending(b => b.CreatedAt)
                .Take(8)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingCode,
                    CustomerName   = b.User.FullName,
                    CourtName      = b.Court.CourtName,
                    CourtTypeName  = b.Court.CourtType.TypeName,
                    BookingDate    = b.BookingDate.ToString("dd/MM/yyyy"),
                    SlotName       = b.TimeSlot.SlotName,
                    b.TotalAmount,
                    Status         = b.Status.ToString()
                })
                .ToListAsync();

            // ── Court Status Grid ──
            var courtGrid = await _context.Courts
                .Include(c => c.CourtType)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CourtTypeId).ThenBy(c => c.CourtCode)
                .Select(c => new
                {
                    c.CourtId,
                    c.CourtName,
                    c.CourtCode,
                    CourtType    = c.CourtType.TypeName,
                    Status       = c.Status.ToString(),
                    c.PricePerHour
                })
                .ToListAsync();

            // ── Operational Alerts ──
            var alerts = new List<object>();
            if (pendingBookings > 0)
                alerts.Add(new { type = "warning", icon = "fa-clock",       message = $"{pendingBookings} booking đang chờ xác nhận" });
            if (lowStockServices > 0)
                alerts.Add(new { type = "warning", icon = "fa-box-open",    message = $"{lowStockServices} dịch vụ tồn kho dưới 10 đơn vị" });
            if (maintenanceCourts > 0)
                alerts.Add(new { type = "info",    icon = "fa-wrench",      message = $"{maintenanceCourts} sân đang trong chế độ bảo trì" });
            if (inactiveCourts > 0)
                alerts.Add(new { type = "danger",  icon = "fa-ban",         message = $"{inactiveCourts} sân đã bị ngưng hoạt động" });
            if (!alerts.Any())
                alerts.Add(new { type = "success", icon = "fa-circle-check",message = "Hệ thống hoạt động bình thường — không có cảnh báo" });

            // ── Top Customers this week ──
            var weekStart    = today.AddDays(-(int)today.DayOfWeek);
            var topCustomers = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.BookingDate >= weekStart && b.Status != BookingStatus.Cancelled)
                .GroupBy(b => new { b.UserId, b.User.FullName })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.FullName,
                    TotalSpend   = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpend)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                kpis = new
                {
                    todayRevenue, monthRevenue,
                    todayBookings, pendingBookings,
                    occupancyRate,
                    activeCustomers, activeCoaches,
                    totalCourts, availableCourts, maintenanceCourts, inactiveCourts,
                    lowStockServices
                },
                revenueChart,
                recentBookings,
                courtGrid,
                alerts,
                topCustomers
            });
        }
    }
}
