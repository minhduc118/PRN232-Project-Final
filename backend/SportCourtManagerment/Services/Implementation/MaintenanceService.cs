using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DataAccess.Interface;
using SportCourtManagerment.DTOs.Maintenance;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;
using SportCourtManagerment.Services.Interface;

namespace SportCourtManagerment.Services.Implementation;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly INotificationRepository _notificationRepo;
    private readonly ApplicationDbContext _db;

    public MaintenanceService(
        IMaintenanceRepository maintenanceRepo,
        IStaffRepository staffRepo,
        INotificationRepository notificationRepo,
        ApplicationDbContext db)
    {
        _maintenanceRepo = maintenanceRepo;
        _staffRepo = staffRepo;
        _notificationRepo = notificationRepo;
        _db = db;
    }

    public async Task<MaintenanceResponse> CreateTaskAsync(int complexId, CreateMaintenanceRequest request)
    {
        // 1. Check if Court exists and belongs to the complex
        var court = await _db.Courts.FirstOrDefaultAsync(c => c.CourtId == request.CourtId);
        if (court == null)
            throw new KeyNotFoundException($"Không tìm thấy sân thể thao #{request.CourtId}.");
        if (court.ComplexId != complexId)
            throw new InvalidOperationException($"Sân #{request.CourtId} không thuộc tổ hợp sân #{complexId}.");

        // 2. If AssignedStaffId is provided, check if staff exists and has staff role
        if (request.AssignedStaffId.HasValue)
        {
            var staff = await _staffRepo.GetStaffWithRolesAsync(request.AssignedStaffId.Value);
            if (staff == null || !staff.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
                throw new InvalidOperationException($"Nhân viên #{request.AssignedStaffId} không hợp lệ hoặc không có vai trò Staff.");
        }

        var task = new MaintenanceSchedule
        {
            CourtId = request.CourtId,
            MaintenanceType = request.MaintenanceType,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            AssignedStaffId = request.AssignedStaffId,
            Reason = request.Reason,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _maintenanceRepo.CreateAsync(task);

        // FR-TS-04 / Notification: Send notification to the assigned staff if applicable
        if (created.AssignedStaffId.HasValue)
        {
            var notification = new Notification
            {
                UserId = created.AssignedStaffId.Value,
                Title = "Công việc bảo trì mới được giao",
                Body = $"Bạn được giao việc bảo trì tại sân {court.CourtName} ({created.MaintenanceType}) từ {created.StartDateTime:dd/MM/yyyy HH:mm} đến {created.EndDateTime:dd/MM/yyyy HH:mm}. Lý do: {created.Reason}",
                Type = NotificationType.System,
                ReferenceId = created.MaintenanceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepo.CreateNotificationAsync(notification);
        }

        // Load navigation properties for response mapping
        var loaded = await _maintenanceRepo.GetByIdAsync(created.MaintenanceId);
        return MapToResponse(loaded!);
    }

    public async Task<MaintenanceResponse> UpdateTaskAsync(int complexId, int id, UpdateMaintenanceRequest request)
    {
        var task = await _maintenanceRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy ca bảo trì #{id}.");

        if (task.Court.ComplexId != complexId)
            throw new UnauthorizedAccessException("Bạn không được phép chỉnh sửa lịch bảo trì của tổ hợp khác.");

        // Check if court belongs to the complex
        var court = await _db.Courts.FirstOrDefaultAsync(c => c.CourtId == request.CourtId);
        if (court == null)
            throw new KeyNotFoundException($"Không tìm thấy sân thể thao #{request.CourtId}.");
        if (court.ComplexId != complexId)
            throw new InvalidOperationException($"Sân #{request.CourtId} không thuộc tổ hợp sân #{complexId}.");

        // Check assigned staff
        if (request.AssignedStaffId.HasValue)
        {
            var staff = await _staffRepo.GetStaffWithRolesAsync(request.AssignedStaffId.Value);
            if (staff == null || !staff.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
                throw new InvalidOperationException($"Nhân viên #{request.AssignedStaffId} không hợp lệ hoặc không có vai trò Staff.");
        }

        var oldStaffId = task.AssignedStaffId;

        task.CourtId = request.CourtId;
        task.MaintenanceType = request.MaintenanceType;
        task.StartDateTime = request.StartDateTime;
        task.EndDateTime = request.EndDateTime;
        task.AssignedStaffId = request.AssignedStaffId;
        task.Reason = request.Reason;
        task.Status = request.Status;
        task.Result = request.Result;

        await _maintenanceRepo.UpdateAsync(task);

        // If assigned staff changed or newly assigned, send notification
        if (task.AssignedStaffId.HasValue && task.AssignedStaffId != oldStaffId)
        {
            var notification = new Notification
            {
                UserId = task.AssignedStaffId.Value,
                Title = "Thay đổi công việc bảo trì được giao",
                Body = $"Công việc bảo trì của bạn tại sân {court.CourtName} đã được cập nhật. Thời gian: {task.StartDateTime:dd/MM/yyyy HH:mm} - {task.EndDateTime:dd/MM/yyyy HH:mm}.",
                Type = NotificationType.System,
                ReferenceId = task.MaintenanceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepo.CreateNotificationAsync(notification);
        }

        var loaded = await _maintenanceRepo.GetByIdAsync(task.MaintenanceId);
        return MapToResponse(loaded!);
    }

    public async Task<MaintenanceResponse> VerifyTaskAsync(int complexId, int id, VerifyMaintenanceRequest request)
    {
        var task = await _maintenanceRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy ca bảo trì #{id}.");

        if (task.Court.ComplexId != complexId)
            throw new UnauthorizedAccessException("Bạn không được phép nghiệm thu lịch bảo trì của tổ hợp khác.");

        if (request.IsApproved)
        {
            task.Status = MaintenanceStatus.Completed;
            task.Result = string.IsNullOrWhiteSpace(request.Note) ? "Đã nghiệm thu đạt yêu cầu" : request.Note;
        }
        else
        {
            task.Status = MaintenanceStatus.Scheduled;
            task.Result = $"Từ chối nghiệm thu: {request.Note}";
        }

        await _maintenanceRepo.UpdateAsync(task);

        // Send notification to the assigned staff about approval/rejection
        if (task.AssignedStaffId.HasValue)
        {
            var title = request.IsApproved ? "Công việc bảo trì đã được nghiệm thu" : "Yêu cầu bảo trì bị từ chối nghiệm thu";
            var body = request.IsApproved 
                ? $"Công việc bảo trì tại sân {task.Court.CourtName} của bạn đã được quản lý phê duyệt. Ghi chú: {task.Result}"
                : $"Yêu cầu nghiệm thu tại sân {task.Court.CourtName} bị từ chối. Lý do: {request.Note}. Vui lòng kiểm tra lại.";

            var notification = new Notification
            {
                UserId = task.AssignedStaffId.Value,
                Title = title,
                Body = body,
                Type = NotificationType.System,
                ReferenceId = task.MaintenanceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepo.CreateNotificationAsync(notification);
        }

        return MapToResponse(task);
    }

    public async Task<MaintenanceResponse> GetByIdAsync(int id)
    {
        var task = await _maintenanceRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy ca bảo trì #{id}.");
        return MapToResponse(task);
    }

    public async Task<PagedMaintenanceResponse> GetTasksAsync(
        int complexId,
        MaintenanceStatus? status = null,
        int page = 1,
        int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var (items, totalCount) = await _maintenanceRepo.GetByComplexAsync(complexId, status, page, pageSize);

        return new PagedMaintenanceResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task DeleteTaskAsync(int complexId, int id)
    {
        var task = await _maintenanceRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy ca bảo trì #{id}.");

        if (task.Court.ComplexId != complexId)
            throw new UnauthorizedAccessException("Bạn không được phép xóa lịch bảo trì của tổ hợp khác.");

        // Business rule: Cannot delete completed task
        if (task.Status == MaintenanceStatus.Completed)
            throw new InvalidOperationException("Không thể xóa công việc bảo trì đã hoàn thành.");

        await _maintenanceRepo.DeleteAsync(task);
    }

    private static MaintenanceResponse MapToResponse(MaintenanceSchedule task)
    {
        return new MaintenanceResponse
        {
            MaintenanceId = task.MaintenanceId,
            CourtId = task.CourtId,
            CourtName = task.Court?.CourtName ?? string.Empty,
            ComplexId = task.Court?.ComplexId ?? 0,
            ComplexName = task.Court?.Complex?.ComplexName ?? string.Empty,
            MaintenanceType = task.MaintenanceType.ToString(),
            StartDateTime = task.StartDateTime,
            EndDateTime = task.EndDateTime,
            AssignedStaffId = task.AssignedStaffId,
            AssignedStaffName = task.AssignedStaff?.FullName,
            Reason = task.Reason,
            Result = task.Result,
            Status = task.Status.ToString(),
            CreatedAt = task.CreatedAt
        };
    }
}
