using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IMaintenanceScheduleRepository
    {
        IEnumerable<MaintenanceSchedule> GetAll();
        MaintenanceSchedule? GetById(int id);
        void Add(MaintenanceSchedule entity);
        void Update(MaintenanceSchedule entity);
        void Delete(int id);
    }
}
