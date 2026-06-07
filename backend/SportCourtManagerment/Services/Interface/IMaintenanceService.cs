using SportCourtManagerment.DTOs.Maintenance;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Services.Interface;

public interface IMaintenanceService
{
    Task<MaintenanceResponse> CreateTaskAsync(int complexId, CreateMaintenanceRequest request);
    Task<MaintenanceResponse> UpdateTaskAsync(int complexId, int id, UpdateMaintenanceRequest request);
    Task<MaintenanceResponse> VerifyTaskAsync(int complexId, int id, VerifyMaintenanceRequest request);
    Task<MaintenanceResponse> GetByIdAsync(int id);
    Task<PagedMaintenanceResponse> GetTasksAsync(
        int complexId,
        MaintenanceStatus? status = null,
        int page = 1,
        int pageSize = 20);
    Task DeleteTaskAsync(int complexId, int id);
}
