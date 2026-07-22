using SportCourtManagent_Server.DTOs.Staff;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IStaffService
    {
        // ─── FR-ST-01: Xem danh sách nhân sự ────────────────────────

        Task<PagedStaffResponse> GetStaffListAsync(int complexId, string? search = null, bool? isActive = null, int page = 1, int pageSize = 10);

        // ─── FR-ST-01b: Assign / Unassign Staff khỏi Complex ────────

        Task AssignStaffToComplexAsync(int complexId, int staffId);

        Task RemoveStaffFromComplexAsync(int complexId, int staffId);

        // ─── FR-ST-02: Xếp ca làm việc ──────────────────────────────

        Task<WeeklyScheduleResponse> GetWeeklyScheduleAsync(int complexId, DateOnly weekStart);
        Task<WeeklyScheduleResponse> GetWeeklyScheduleByStaffAsync(int complexId, int staffId, DateOnly weekStart);

        Task<StaffShiftResponse> CreateShiftAsync(int complexId, CreateShiftRequest request);

        Task<StaffShiftResponse> UpdateShiftAsync(int complexId, int shiftId, UpdateShiftRequest request);

        Task DeleteShiftAsync(int complexId, int shiftId);

        Task<StaffShiftResponse> GetShiftByIdAsync(int shiftId);

        // ─── FR-ST-03: Theo dõi chấm công ───────────────────────────

        Task<StaffShiftResponse> CheckInShiftAsync(int staffId, int shiftId);

        Task<StaffShiftResponse> CheckOutShiftAsync(int staffId, int shiftId);

        Task<List<StaffShiftResponse>> GetAttendanceReportAsync(int complexId, DateOnly? dateFrom, DateOnly? dateTo, int? staffId = null);
    }
}
