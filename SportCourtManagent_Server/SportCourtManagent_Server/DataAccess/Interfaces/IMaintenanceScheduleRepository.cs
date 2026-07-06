using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using System.Collections.Generic;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IMaintenanceScheduleRepository
    {
        Task<MaintenanceSchedule?> GetByIdAsync(int id);
        Task<(List<MaintenanceSchedule> Items, int TotalCount)> GetByComplexAsync(int complexId, MaintenanceStatus? status = null, int page = 1, int pageSize = 10);
        Task<MaintenanceSchedule> CreateAsync(MaintenanceSchedule schedule);
        Task<bool> UpdateAsync(MaintenanceSchedule schedule);
        Task<bool> DeleteAsync(MaintenanceSchedule schedule);
    }
}
