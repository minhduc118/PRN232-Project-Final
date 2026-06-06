using SportCourtManagerment.DataAccess.Interface;
using SportCourtManagerment.DTOs.Staff;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;
using SportCourtManagerment.Services.Interface;

namespace SportCourtManagerment.Services.Implementation;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepo;
    private readonly IStaffShiftRepository _shiftRepo;

    private static readonly Dictionary<ShiftType, (TimeOnly Start, TimeOnly End)> ShiftTimes = new()
    {
        [ShiftType.Morning] = (new TimeOnly(6, 0), new TimeOnly(14, 0)),
        [ShiftType.Afternoon] = (new TimeOnly(14, 0), new TimeOnly(22, 0)),
        [ShiftType.Evening] = (new TimeOnly(22, 0), new TimeOnly(6, 0)), // qua đêm
    };

    public StaffService(IStaffRepository staffRepo, IStaffShiftRepository shiftRepo)
    {
        _staffRepo = staffRepo;
        _shiftRepo = shiftRepo;
    }

    // ─── FR-ST-01 ─────────────────────────────────────────────────

    public async Task<PagedStaffResponse> GetStaffListAsync(
      int complexId,
      string? search = null,
      bool? isActive = null,
      int page = 1,
      int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var (staffList, totalCount) = await _staffRepo.GetStaffByComplexAsync(
          complexId, search, isActive, page, pageSize);

        var items = new List<StaffSummaryResponse>();
        foreach (var staff in staffList)
        {
            var todayShift = await _shiftRepo.GetTodayShiftAsync(staff.UserId);
            var shiftsThisWeek = await _shiftRepo.CountShiftsThisWeekAsync(staff.UserId);

            items.Add(MapToStaffSummaryResponse(staff, todayShift, shiftsThisWeek));
        }

        return new PagedStaffResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    // ─── FR-ST-02 ─────────────────────────────────────────────────

    public async Task<WeeklyScheduleResponse> GetWeeklyScheduleAsync(int complexId, DateOnly weekStart)
    {
        // Chuẩn hóa về Monday (ISO week)
        var dayOfWeek = (int)weekStart.DayOfWeek; // 0 = Sunday
        var monday = weekStart.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
        var sunday = monday.AddDays(6);

        var shifts = await _shiftRepo.GetShiftsByComplexAndDateRangeAsync(complexId, monday, sunday);

        var days = new List<DailyShiftGroupResponse>();
        for (var d = monday; d <= sunday; d = d.AddDays(1))
        {
            var dayShifts = shifts
              .Where(ss => ss.ShiftDate == d)
              .Select(MapToShiftResponse)
              .ToList();

            days.Add(new DailyShiftGroupResponse
            {
                Date = d.ToString("yyyy-MM-dd"),
                DayName = d.DayOfWeek.ToString(),
                Shifts = dayShifts
            });
        }

        return new WeeklyScheduleResponse
        {
            WeekStart = monday.ToString("yyyy-MM-dd"),
            WeekEnd = sunday.ToString("yyyy-MM-dd"),
            Days = days
        };
    }

    public async Task<StaffShiftResponse> CreateShiftAsync(int complexId, CreateShiftRequest request)
    {
        // 1. Validate Staff tồn tại và có role "Staff"
        var staff = await _staffRepo.GetStaffWithRolesAsync(request.StaffId)
          ?? throw new KeyNotFoundException($"Không tìm thấy nhân viên với mã #{request.StaffId}.");

        if (!staff.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
            throw new InvalidOperationException($"Người dùng #{request.StaffId} không có vai trò Staff.");

        // 2. Validate trùng ca: (StaffId, ShiftDate, ShiftType) unique
        var exists = await _shiftRepo.ExistsAsync(request.StaffId, request.ShiftDate, request.ShiftType);
        if (exists)
            throw new InvalidOperationException(
              $"Nhân viên #{request.StaffId} đã có ca {request.ShiftType} " +
              $"vào ngày {request.ShiftDate:yyyy-MM-dd}. " +
              "Mỗi nhân viên chỉ được xếp một ca cùng loại trong một ngày.");

        // 3. Xác định StartTime / EndTime từ ShiftType
        var (startTime, endTime) = ShiftTimes[request.ShiftType];

        var shift = new StaffShift
        {
            StaffId = request.StaffId,
            ShiftDate = request.ShiftDate,
            ShiftType = request.ShiftType,
            StartTime = startTime,
            EndTime = endTime,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _shiftRepo.CreateAsync(shift);
        return MapToShiftResponse(created);
    }

    public async Task<BulkCreateShiftResponse> CreateShiftBulkAsync(
      int complexId, BulkCreateShiftRequest request)
    {
        var result = new BulkCreateShiftResponse();
        var toCreate = new List<StaffShift>();

        foreach (var item in request.Shifts)
        {
            // Validate Staff
            var staff = await _staffRepo.GetStaffWithRolesAsync(item.StaffId);
            if (staff is null)
            {
                result.Errors.Add($"Nhân viên #{item.StaffId} không tồn tại.");
                result.Skipped++;
                continue;
            }

            if (!staff.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
            {
                result.Errors.Add(
                  $"Người dùng #{item.StaffId} ({staff.FullName}) không có vai trò Staff.");
                result.Skipped++;
                continue;
            }

            // Validate trùng ca
            var exists = await _shiftRepo.ExistsAsync(item.StaffId, item.ShiftDate, item.ShiftType);
            if (exists)
            {
                result.Errors.Add(
                  $"Ca {item.ShiftType} ngày {item.ShiftDate:yyyy-MM-dd} " +
                  $"của nhân viên #{item.StaffId} ({staff.FullName}) đã tồn tại.");
                result.Skipped++;
                continue;
            }

            var (startTime, endTime) = ShiftTimes[item.ShiftType];
            toCreate.Add(new StaffShift
            {
                StaffId = item.StaffId,
                ShiftDate = item.ShiftDate,
                ShiftType = item.ShiftType,
                StartTime = startTime,
                EndTime = endTime,
                Note = item.Note,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (toCreate.Count > 0)
        {
            var created = await _shiftRepo.CreateBulkAsync(toCreate);
            result.Created = created.Count;
            result.CreatedShifts = created.Select(MapToShiftResponse).ToList();
        }

        return result;
    }

    public async Task<StaffShiftResponse> UpdateShiftAsync(
      int complexId, int shiftId, UpdateShiftRequest request)
    {
        var shift = await _shiftRepo.GetByIdAsync(shiftId)
          ?? throw new KeyNotFoundException($"Không tìm thấy ca làm việc #{shiftId}.");

        // Validate trùng nếu đổi ShiftType
        if (shift.ShiftType != request.ShiftType)
        {
            var exists = await _shiftRepo.ExistsAsync(shift.StaffId, shift.ShiftDate, request.ShiftType);
            if (exists)
                throw new InvalidOperationException(
                  $"Nhân viên #{shift.StaffId} đã có ca {request.ShiftType} " +
                  $"vào ngày {shift.ShiftDate:yyyy-MM-dd}.");
        }

        var (startTime, endTime) = ShiftTimes[request.ShiftType];
        shift.ShiftType = request.ShiftType;
        shift.StartTime = startTime;
        shift.EndTime = endTime;
        shift.Note = request.Note;

        var success = await _shiftRepo.UpdateAsync(shift);
        if (!success)
            throw new InvalidOperationException("Cập nhật ca làm việc thất bại.");
        return MapToShiftResponse(shift);
    }

    public async Task DeleteShiftAsync(int complexId, int shiftId)
    {
        var shift = await _shiftRepo.GetByIdAsync(shiftId)
          ?? throw new KeyNotFoundException($"Không tìm thấy ca làm việc #{shiftId}.");

        // Business rule: Không được xóa ca khi nhân viên đã check-in
        if (shift.CheckInTime.HasValue)
            throw new InvalidOperationException(
              "Không thể xóa ca làm việc khi nhân viên đã thực hiện check-in. " +
              "Hãy liên hệ quản trị viên.");

        var success = await _shiftRepo.DeleteAsync(shift);
        if (!success)
            throw new InvalidOperationException("Xóa ca làm việc thất bại.");
    }

    public async Task<StaffShiftResponse> GetShiftByIdAsync(int shiftId)
    {
        var shift = await _shiftRepo.GetByIdAsync(shiftId)
          ?? throw new KeyNotFoundException($"Không tìm thấy ca làm việc #{shiftId}.");

        return MapToShiftResponse(shift);
    }

    // ─── Private Mapping Helpers ──────────────────────────────────

    private static StaffSummaryResponse MapToStaffSummaryResponse(
      User staff,
      StaffShift? todayShift,
      int shiftsThisWeek)
    {
        return new StaffSummaryResponse
        {
            UserId = staff.UserId,
            FullName = staff.FullName,
            Email = staff.Email,
            Phone = staff.Phone,
            AvatarUrl = staff.AvatarUrl,
            IsActive = staff.IsActive,
            ShiftsThisWeek = shiftsThisWeek,
            TodayShift = todayShift is null ? null : new ShiftSummaryResponse
            {
                ShiftId = todayShift.ShiftId,
                ShiftType = todayShift.ShiftType.ToString(),
                StartTime = todayShift.StartTime.ToString("HH:mm"),
                EndTime = todayShift.EndTime.ToString("HH:mm"),
                CheckInTime = todayShift.CheckInTime,
                CheckOutTime = todayShift.CheckOutTime
            }
        };
    }

    private static StaffShiftResponse MapToShiftResponse(StaffShift shift)
    {
        return new StaffShiftResponse
        {
            ShiftId = shift.ShiftId,
            StaffId = shift.StaffId,
            StaffName = shift.Staff?.FullName ?? string.Empty,
            StaffEmail = shift.Staff?.Email ?? string.Empty,
            AvatarUrl = shift.Staff?.AvatarUrl,
            ShiftDate = shift.ShiftDate.ToString("yyyy-MM-dd"),
            ShiftType = shift.ShiftType.ToString(),
            StartTime = shift.StartTime.ToString("HH:mm"),
            EndTime = shift.EndTime.ToString("HH:mm"),
            CheckInTime = shift.CheckInTime,
            CheckOutTime = shift.CheckOutTime,
            Note = shift.Note,
            CreatedAt = shift.CreatedAt
        };
    }
}
