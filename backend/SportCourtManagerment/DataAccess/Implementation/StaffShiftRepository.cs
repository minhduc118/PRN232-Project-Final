using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DataAccess.Interface;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Implementation;

public class StaffShiftRepository : IStaffShiftRepository
{
    private readonly ApplicationDbContext _db;

    public StaffShiftRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    // ─── Queries ──────────────────────────────────────────────────

    public async Task<StaffShift?> GetByIdAsync(int shiftId)
    {
        return await _db.StaffShifts
          .Include(ss => ss.Staff)
          .FirstOrDefaultAsync(ss => ss.ShiftId == shiftId);
    }

    public async Task<List<StaffShift>> GetShiftsByComplexAndDateRangeAsync(
      int complexId,
      DateOnly dateFrom,
      DateOnly dateTo)
    {
        // TODO: Khi StaffShift có ComplexId, lọc trực tiếp theo complexId
        var staffIds = await _db.Users
          .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Staff") && u.IsActive)
          .Select(u => u.UserId)
          .ToListAsync();

        return await _db.StaffShifts
          .Include(ss => ss.Staff)
          .Where(ss =>
            staffIds.Contains(ss.StaffId) &&
            ss.ShiftDate >= dateFrom &&
            ss.ShiftDate <= dateTo)
          .OrderBy(ss => ss.ShiftDate)
          .ThenBy(ss => ss.ShiftType)
          .ToListAsync();
    }

    public async Task<List<StaffShift>> GetShiftsByStaffAndDateRangeAsync(
      int staffId,
      DateOnly dateFrom,
      DateOnly dateTo)
    {
        return await _db.StaffShifts
          .Include(ss => ss.Staff)
          .Where(ss =>
            ss.StaffId == staffId &&
            ss.ShiftDate >= dateFrom &&
            ss.ShiftDate <= dateTo)
          .OrderBy(ss => ss.ShiftDate)
          .ToListAsync();
    }

    public async Task<StaffShift?> GetTodayShiftAsync(int staffId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.StaffShifts
          .Include(ss => ss.Staff)
          .FirstOrDefaultAsync(ss => ss.StaffId == staffId && ss.ShiftDate == today);
    }

    public async Task<int> CountShiftsThisWeekAsync(int staffId)
    {
        var today = DateTime.Today;
        var dayOfWeek = (int)today.DayOfWeek; // 0 = Sunday
        var monday = DateOnly.FromDateTime(today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1)));
        var sunday = monday.AddDays(6);

        return await _db.StaffShifts
          .CountAsync(ss =>
            ss.StaffId == staffId &&
            ss.ShiftDate >= monday &&
            ss.ShiftDate <= sunday);
    }

    public async Task<bool> ExistsAsync(int staffId, DateOnly shiftDate, ShiftType shiftType)
    {
        return await _db.StaffShifts
          .AnyAsync(ss =>
            ss.StaffId == staffId &&
            ss.ShiftDate == shiftDate &&
            ss.ShiftType == shiftType);
    }

    // ─── Commands ─────────────────────────────────────────────────

    public async Task<StaffShift> CreateAsync(StaffShift shift)
    {
        _db.StaffShifts.Add(shift);
        await _db.SaveChangesAsync();
        await _db.Entry(shift).Reference(ss => ss.Staff).LoadAsync();
        return shift;
    }

    public async Task<List<StaffShift>> CreateBulkAsync(List<StaffShift> shifts)
    {
        _db.StaffShifts.AddRange(shifts);
        await _db.SaveChangesAsync();

        foreach (var shift in shifts)
            await _db.Entry(shift).Reference(ss => ss.Staff).LoadAsync();

        return shifts;
    }

    public async Task<bool> UpdateAsync(StaffShift shift)
    {
        _db.StaffShifts.Update(shift);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(StaffShift shift)
    {
        _db.StaffShifts.Remove(shift);
        return await _db.SaveChangesAsync() > 0;
    }
}