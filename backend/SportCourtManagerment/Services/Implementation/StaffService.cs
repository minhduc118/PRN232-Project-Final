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
    private readonly INotificationRepository _notificationRepo;

    private static readonly Dictionary<ShiftType, (TimeOnly Start, TimeOnly End)> ShiftTimes = new()
    {
        [ShiftType.Morning] = (new TimeOnly(6, 0), new TimeOnly(14, 0)),
        [ShiftType.Afternoon] = (new TimeOnly(14, 0), new TimeOnly(22, 0)),
        [ShiftType.Evening] = (new TimeOnly(22, 0), new TimeOnly(6, 0)), // qua đêm
    };

    public StaffService(
        IStaffRepository staffRepo,
        IStaffShiftRepository shiftRepo,
        INotificationRepository notificationRepo)
    {
        _staffRepo = staffRepo;
        _shiftRepo = shiftRepo;
        _notificationRepo = notificationRepo;
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

        // FR-ST-04: Tạo thông báo gán cho UserId của Staff
        var notification = new Notification
        {
            UserId = created.StaffId,
            Title = "Lịch làm việc mới",
            Body = $"Bạn đã được phân công ca {created.ShiftType} ngày {created.ShiftDate:yyyy-MM-dd} ({created.StartTime:HH:mm} - {created.EndTime:HH:mm}).",
            Type = NotificationType.System,
            ReferenceId = created.ShiftId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.CreateNotificationAsync(notification);

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

            // FR-ST-04: Tạo thông báo gán cho UserId của các Staff trong danh sách
            var notifications = created.Select(s => new Notification
            {
                UserId = s.StaffId,
                Title = "Lịch làm việc mới",
                Body = $"Bạn đã được phân công ca {s.ShiftType} ngày {s.ShiftDate:yyyy-MM-dd} ({s.StartTime:HH:mm} - {s.EndTime:HH:mm}).",
                Type = NotificationType.System,
                ReferenceId = s.ShiftId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _notificationRepo.CreateNotificationsBulkAsync(notifications);
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

        // FR-ST-04: Tạo thông báo khi ca làm việc thay đổi
        var notification = new Notification
        {
            UserId = shift.StaffId,
            Title = "Lịch làm việc thay đổi",
            Body = $"Ca làm việc của bạn ngày {shift.ShiftDate:yyyy-MM-dd} đã được đổi thành ca {shift.ShiftType} ({shift.StartTime:HH:mm} - {shift.EndTime:HH:mm}).",
            Type = NotificationType.System,
            ReferenceId = shift.ShiftId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.CreateNotificationAsync(notification);

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

        // Lưu thông tin để tạo thông báo
        var staffId = shift.StaffId;
        var shiftDate = shift.ShiftDate;
        var shiftType = shift.ShiftType;

        var success = await _shiftRepo.DeleteAsync(shift);
        if (!success)
            throw new InvalidOperationException("Xóa ca làm việc thất bại.");

        // FR-ST-04: Tạo thông báo khi ca làm việc bị xóa
        var notification = new Notification
        {
            UserId = staffId,
            Title = "Lịch làm việc đã hủy",
            Body = $"Ca làm việc {shiftType} ngày {shiftDate:yyyy-MM-dd} của bạn đã bị hủy.",
            Type = NotificationType.System,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.CreateNotificationAsync(notification);
    }

    public async Task<StaffShiftResponse> GetShiftByIdAsync(int shiftId)
    {
        var shift = await _shiftRepo.GetByIdAsync(shiftId)
          ?? throw new KeyNotFoundException($"Không tìm thấy ca làm việc #{shiftId}.");

        return MapToShiftResponse(shift);
    }

    // ─── FR-ST-03: Theo dõi chấm công ───────────────────────────

    public async Task<StaffShiftResponse> CheckInShiftAsync(int staffId, int shiftId)
    {
        var shift = await _shiftRepo.GetByIdAsync(shiftId)
          ?? throw new KeyNotFoundException($"Không tìm thấy ca làm việc #{shiftId}.");

        if (shift.StaffId != staffId)
            throw new UnauthorizedAccessException("Bạn không được phép check-in ca làm việc của nhân viên khác.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (shift.ShiftDate != today)
            throw new InvalidOperationException("Chỉ có thể check-in ca làm việc của ngày hôm nay.");

        if (shift.CheckInTime.HasValue)
            throw new InvalidOperationException("Bạn đã thực hiện check-in ca làm việc này rồi.");

        shift.CheckInTime = DateTime.UtcNow;
        var success = await _shiftRepo.UpdateAsync(shift);
        if (!success)
            throw new InvalidOperationException("Check-in thất bại.");

        return MapToShiftResponse(shift);
    }

    public async Task<StaffShiftResponse> CheckOutShiftAsync(int staffId, int shiftId)
    {
        var shift = await _shiftRepo.GetByIdAsync(shiftId)
          ?? throw new KeyNotFoundException($"Không tìm thấy ca làm việc #{shiftId}.");

        if (shift.StaffId != staffId)
            throw new UnauthorizedAccessException("Bạn không được phép check-out ca làm việc của nhân viên khác.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var isAllowedDate = shift.ShiftDate == today || 
                            (shift.ShiftDate.AddDays(1) == today && shift.ShiftType == ShiftType.Evening);
        if (!isAllowedDate)
            throw new InvalidOperationException("Chỉ có thể check-out ca làm việc của ngày hôm nay.");

        if (!shift.CheckInTime.HasValue)
            throw new InvalidOperationException("Bạn phải check-in trước khi check-out.");

        if (shift.CheckOutTime.HasValue)
            throw new InvalidOperationException("Bạn đã thực hiện check-out ca làm việc này rồi.");

        shift.CheckOutTime = DateTime.UtcNow;
        var success = await _shiftRepo.UpdateAsync(shift);
        if (!success)
            throw new InvalidOperationException("Check-out thất bại.");

        return MapToShiftResponse(shift);
    }

    public async Task<List<StaffShiftResponse>> GetAttendanceReportAsync(
      int complexId,
      DateOnly? dateFrom,
      DateOnly? dateTo,
      int? staffId = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dayOfWeek = (int)today.DayOfWeek;
        var start = dateFrom ?? today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
        var end = dateTo ?? start.AddDays(6);

        List<StaffShift> shifts;
        if (staffId.HasValue)
        {
            shifts = await _shiftRepo.GetShiftsByStaffAndDateRangeAsync(staffId.Value, start, end);
        }
        else
        {
            shifts = await _shiftRepo.GetShiftsByComplexAndDateRangeAsync(complexId, start, end);
        }

        return shifts.Select(MapToShiftResponse).ToList();
    }

    // ─── Private Mapping Helpers ──────────────────────────────────

    private static StaffSummaryResponse MapToStaffSummaryResponse(
      User staff,
      StaffShift? todayShift,
      int shiftsThisWeek)
    {
        ShiftSummaryResponse? todayShiftDto = null;
        if (todayShift != null)
        {
            todayShiftDto = new ShiftSummaryResponse
            {
                ShiftId = todayShift.ShiftId,
                ShiftType = todayShift.ShiftType.ToString(),
                StartTime = todayShift.StartTime.ToString("HH:mm"),
                EndTime = todayShift.EndTime.ToString("HH:mm"),
                CheckInTime = todayShift.CheckInTime,
                CheckOutTime = todayShift.CheckOutTime
            };

            if (todayShift.CheckInTime.HasValue)
            {
                var scheduledStartLocal = todayShift.ShiftDate.ToDateTime(todayShift.StartTime);
                var checkInLocal = todayShift.CheckInTime.Value.AddHours(7);
                if (checkInLocal > scheduledStartLocal)
                {
                    todayShiftDto.LateMinutes = (int)Math.Max(0, (checkInLocal - scheduledStartLocal).TotalMinutes);
                }
            }

            if (todayShift.CheckOutTime.HasValue)
            {
                var scheduledEndLocal = todayShift.ShiftDate.ToDateTime(todayShift.EndTime);
                if (todayShift.EndTime < todayShift.StartTime)
                {
                    scheduledEndLocal = todayShift.ShiftDate.AddDays(1).ToDateTime(todayShift.EndTime);
                }
                var checkOutLocal = todayShift.CheckOutTime.Value.AddHours(7);
                if (checkOutLocal < scheduledEndLocal)
                {
                    todayShiftDto.EarlyLeaveMinutes = (int)Math.Max(0, (scheduledEndLocal - checkOutLocal).TotalMinutes);
                }
            }
        }

        return new StaffSummaryResponse
        {
            UserId = staff.UserId,
            FullName = staff.FullName,
            Email = staff.Email,
            Phone = staff.Phone,
            AvatarUrl = staff.AvatarUrl,
            IsActive = staff.IsActive,
            ShiftsThisWeek = shiftsThisWeek,
            TodayShift = todayShiftDto
        };
    }

    private static StaffShiftResponse MapToShiftResponse(StaffShift shift)
    {
        var response = new StaffShiftResponse
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

        if (shift.CheckInTime.HasValue)
        {
            var scheduledStartLocal = shift.ShiftDate.ToDateTime(shift.StartTime);
            var checkInLocal = shift.CheckInTime.Value.AddHours(7);
            if (checkInLocal > scheduledStartLocal)
            {
                response.LateMinutes = (int)Math.Max(0, (checkInLocal - scheduledStartLocal).TotalMinutes);
            }
        }

        if (shift.CheckOutTime.HasValue)
        {
            var scheduledEndLocal = shift.ShiftDate.ToDateTime(shift.EndTime);
            if (shift.EndTime < shift.StartTime)
            {
                scheduledEndLocal = shift.ShiftDate.AddDays(1).ToDateTime(shift.EndTime);
            }
            var checkOutLocal = shift.CheckOutTime.Value.AddHours(7);
            if (checkOutLocal < scheduledEndLocal)
            {
                response.EarlyLeaveMinutes = (int)Math.Max(0, (scheduledEndLocal - checkOutLocal).TotalMinutes);
            }
        }

        return response;
    }
}
