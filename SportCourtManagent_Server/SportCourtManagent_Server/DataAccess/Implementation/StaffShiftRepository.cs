using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class StaffShiftRepository : IStaffShiftRepository
    {
        private readonly AppDbContext _context;

        public StaffShiftRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountShiftsThisWeekAsync(int staffId)
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek; // 0 = Sunday
            var monday = DateOnly.FromDateTime(today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1)));
            var sunday = monday.AddDays(6);

            return await _context.StaffShifts
              .CountAsync(ss => ss.StaffId == staffId && ss.ShiftDate >= monday && ss.ShiftDate <= sunday);
        }

        public async Task<StaffShift> CreateAsync(StaffShift shift)
        {
            await _context.StaffShifts.AddAsync(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<List<StaffShift>> CreateBulkAsync(List<StaffShift> shifts)
        {
            if (shifts == null || shifts.Count == 0)
            {
                return new List<StaffShift>();
            }
            await _context.StaffShifts.AddRangeAsync(shifts);
            await _context.SaveChangesAsync();
            return shifts;
        }

        public async Task<bool> DeleteAsync(StaffShift shift)
        {
            _context.StaffShifts.Remove(shift);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(int staffId, DateOnly shiftDate, ShiftType shiftType)
        {
            return await _context.StaffShifts
                .AnyAsync(ss => ss.StaffId == staffId && ss.ShiftDate == shiftDate && ss.ShiftType == shiftType);
        }

        public async Task<StaffShift?> GetByIdAsync(int shiftId)
        {
            return await _context.StaffShifts
                .Include(ss => ss.Staff).ThenInclude(s => s.UserRoles).ThenInclude(ur => ur.Role)
                .Include(ss => ss.Complex)
                .FirstOrDefaultAsync(ss => ss.ShiftId == shiftId);
        }

        public async Task<List<StaffShift>> GetShiftsByComplexAndDateRangeAsync(int complexId, DateOnly dateFrom, DateOnly dateTo)
        {
            return await _context.StaffShifts
                .Include(ss => ss.Staff).ThenInclude(s => s.UserRoles).ThenInclude(ur => ur.Role)
                .Include(ss => ss.Complex)
                .Where(ss => ss.ComplexId == complexId && ss.ShiftDate >= dateFrom && ss.ShiftDate <= dateTo)
                .ToListAsync();
        }

        public async Task<List<StaffShift>> GetShiftsByStaffAndDateRangeAsync(int staffId, DateOnly dateFrom, DateOnly dateTo)
        {
            return await _context.StaffShifts
                .Include(ss => ss.Staff).ThenInclude(s => s.UserRoles).ThenInclude(ur => ur.Role)
                .Include(ss => ss.Complex)
                .Where(ss => ss.StaffId == staffId && ss.ShiftDate >= dateFrom && ss.ShiftDate <= dateTo)
                .ToListAsync();
        }

        public async Task<StaffShift?> GetTodayShiftAsync(int staffId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.StaffShifts
                .Include(ss => ss.Staff).ThenInclude(s => s.UserRoles).ThenInclude(ur => ur.Role)
                .Include(ss => ss.Complex)
                .OrderBy(ss => ss.StartTime)
                .FirstOrDefaultAsync(ss => ss.StaffId == staffId && ss.ShiftDate == today);
        }

        public async Task<bool> UpdateAsync(StaffShift shift)
        {
            _context.StaffShifts.Update(shift);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
