using System;
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
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaffDashboardController(AppDbContext context)
        {
            _context = context;
        }

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

            // Get a list of today's bookings to display on dashboard as recent activity
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

            return Ok(new 
            { 
                data = stats,
                recentBookings = recentBookings
            });
        }
    }
}
