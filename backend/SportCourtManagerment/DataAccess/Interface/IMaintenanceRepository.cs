using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Interface;

public interface IMaintenanceRepository
{
    Task<MaintenanceSchedule?> GetByIdAsync(int id);
    Task<(List<MaintenanceSchedule> Items, int TotalCount)> GetByComplexAsync(
        int complexId,
        MaintenanceStatus? status = null,
        int page = 1,
        int pageSize = 20);
    Task<MaintenanceSchedule> CreateAsync(MaintenanceSchedule schedule);
    Task<bool> UpdateAsync(MaintenanceSchedule schedule);
    Task<bool> DeleteAsync(MaintenanceSchedule schedule);
}
