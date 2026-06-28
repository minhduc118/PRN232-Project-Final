using SportCourtManagent_Server.DTOs.Maintenance;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IMaintenanceScheduleService
    {
        Task<MaintenanceResponse> CreateMaintenanceAsync(int complexId, CreateMaintenanceRequest request);
        Task<MaintenanceResponse> UpdateMaintenanceAsync(int complexId, int maintenanceId, UpdateMaintenanceRequest request);
        Task<MaintenanceResponse> VerifyMaintenanceAsync(int complexId, int maintenanceId, VerifyMaintenanceRequest request);
        Task<PagedMaintenanceResponse> GetMaintenanceListAsync(int complexId, MaintenanceStatus? status = null, int page = 1, int pageSize = 20);
        Task<MaintenanceResponse> GetMaintenanceByIdAsync(int maintenanceId);
        Task DeleteMaintenanceAsync(int complexId, int maintenanceId);
    }
}
