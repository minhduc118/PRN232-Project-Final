using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;

namespace SportCourtManagent_Server.Services.Interfaces
{
    /// <summary>
    /// Business logic for court search, detail, availability, and CRUD management.
    /// </summary>
    public interface ICourtService
    {
        /// <summary>Searches courts with filters, sorting, and pagination.</summary>
        Task<PagedResult<CourtListDto>> SearchCourtsAsync(CourtSearchParams searchParams);

        /// <summary>Returns full court detail by ID, or null if not found.</summary>
        Task<CourtDetailDto?> GetCourtDetailAsync(int courtId);

        /// <summary>Returns time slot availability for a court on a specific date.</summary>
        Task<CourtAvailabilityDto?> GetCourtAvailabilityAsync(int courtId, DateTime date);

        /// <summary>Returns an IQueryable of courts for OData endpoint.</summary>
        IQueryable<CourtListDto> GetCourtsODataQueryable();

        Task<IEnumerable<CourtDto>> GetAllAsync(int? complexId, string? status);
        Task<CourtDto?> GetByIdAsync(int id);
        Task<CourtDto> CreateAsync(CourtDto dto);
        Task UpdateAsync(int id, CourtDto dto);

        /// <summary>Ngưng hoạt động (Inactive). Chỉ khi không còn booking Pending/Confirmed.</summary>
        Task<CourtLifecycleResultDto> DeactivateAsync(int id);

        /// <summary>Khôi phục sân về Available.</summary>
        Task<CourtLifecycleResultDto> RestoreAsync(int id);

        /// <summary>Xem trước booking bị conflict khi bảo trì theo khung giờ.</summary>
        Task<MaintenanceConflictPreviewDto> PreviewMaintenanceConflictsAsync(int courtId, DateTime start, DateTime end);

        /// <summary>Lên lịch bảo trì: chặn slot, hủy+refund booking conflict nếu ConfirmRefund.</summary>
        Task<CourtLifecycleResultDto> ScheduleMaintenanceAsync(int courtId, ScheduleCourtMaintenanceRequest request);

        [Obsolete("Dùng DeactivateAsync — không soft-delete sân nữa.")]
        Task DeleteAsync(int id);

        Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null);
    }
}
