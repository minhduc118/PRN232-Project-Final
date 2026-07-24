using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class MaintenanceScheduleRepository : IMaintenanceScheduleRepository
    {
        private readonly AppDbContext _context;

        public MaintenanceScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MaintenanceSchedule> CreateAsync(MaintenanceSchedule schedule)
        {
            _context.MaintenanceSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<bool> DeleteAsync(MaintenanceSchedule schedule)
        {
            _context.MaintenanceSchedules.Remove(schedule);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<(List<MaintenanceSchedule> Items, int TotalCount)> GetByComplexAsync(int complexId, MaintenanceStatus? status = null, int? assignedStaffId = null, int page = 1, int pageSize = 10)
        {
            var query = _context.MaintenanceSchedules
                .Include(ms => ms.Court)
                .ThenInclude(c => c.Complex)
                .Include(ms => ms.AssignedStaff)
                .Where(ms => ms.Court.ComplexId == complexId)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(ms => ms.Status == status.Value);
            }

            if (assignedStaffId.HasValue)
            {
                query = query.Where(ms => ms.AssignedStaffId == assignedStaffId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(ms => ms.StartDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<MaintenanceSchedule?> GetByIdAsync(int id)
        {
            return await _context.MaintenanceSchedules
                .Include(ms => ms.Court)
                .ThenInclude(c => c.Complex)
                .Include(ms => ms.AssignedStaff)
                .FirstOrDefaultAsync(ms => ms.MaintenanceId == id);
        }

        public async Task<bool> UpdateAsync(MaintenanceSchedule schedule)
        {
            _context.MaintenanceSchedules.Update(schedule);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
