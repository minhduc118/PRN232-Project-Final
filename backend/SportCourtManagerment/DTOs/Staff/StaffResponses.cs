namespace SportCourtManagerment.DTOs.Staff;

// ════════════════════════════════════════════════════════════════════
//  RESPONSE DTOs — FR-ST-01 & FR-ST-02
//  Dùng để trả về data cho client qua ApiResponse<T>
// ════════════════════════════════════════════════════════════════════

// ─── FR-ST-01: Danh sách nhân sự ────────────────────────────────

public class StaffSummaryResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public ShiftSummaryResponse? TodayShift { get; set; }
    public int ShiftsThisWeek { get; set; }
}

public class PagedStaffResponse
{
    public List<StaffSummaryResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// ─── FR-ST-02: Ca làm việc ──────────────────────────────────────

public class ShiftSummaryResponse
{
    public int ShiftId { get; set; }
    public string ShiftType { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
}

public class StaffShiftResponse
{
    public int ShiftId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string StaffEmail { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public string ShiftDate { get; set; } = string.Empty;
    public string ShiftType { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }

    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WeeklyScheduleResponse
{
    public string WeekStart { get; set; } = string.Empty;
    public string WeekEnd { get; set; } = string.Empty;
    public List<DailyShiftGroupResponse> Days { get; set; } = new();
}

public class DailyShiftGroupResponse
{
    public string Date { get; set; } = string.Empty;
    public string DayName { get; set; } = string.Empty;
    public List<StaffShiftResponse> Shifts { get; set; } = new();
}

public class BulkCreateShiftResponse
{
    public int Created { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<StaffShiftResponse> CreatedShifts { get; set; } = new();
}
