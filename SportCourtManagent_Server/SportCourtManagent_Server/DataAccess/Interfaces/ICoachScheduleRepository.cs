using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICoachScheduleRepository
    {
        IEnumerable<CoachSchedule> GetAll();
        CoachSchedule? GetById(int id);
        void Add(CoachSchedule entity);
        void Update(CoachSchedule entity);
        void Delete(int id);
    }
}
