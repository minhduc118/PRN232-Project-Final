using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Interface;

public interface IStaffShiftRepository
{
    // ─── Queries ──────────────────────────────────────────────────

    Task<StaffShift?> GetByIdAsync(int shiftId);

    Task<List<StaffShift>> GetShiftsByComplexAndDateRangeAsync(
      int complexId,
      DateOnly dateFrom,
      DateOnly dateTo);

    Task<List<StaffShift>> GetShiftsByStaffAndDateRangeAsync(
      int staffId,
      DateOnly dateFrom,
      DateOnly dateTo);

    Task<StaffShift?> GetTodayShiftAsync(int staffId);

    Task<int> CountShiftsThisWeekAsync(int staffId);

    Task<bool> ExistsAsync(int staffId, DateOnly shiftDate, ShiftType shiftType);

    // ─── Commands ─────────────────────────────────────────────────

    Task<StaffShift> CreateAsync(StaffShift shift);

    Task<List<StaffShift>> CreateBulkAsync(List<StaffShift> shifts);

    Task<bool> UpdateAsync(StaffShift shift);

    Task<bool> DeleteAsync(StaffShift shift);
}
