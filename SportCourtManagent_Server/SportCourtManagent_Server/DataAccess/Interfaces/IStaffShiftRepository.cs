using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IStaffShiftRepository
    {
        IEnumerable<StaffShift> GetAll();
        StaffShift? GetById(int id);
        void Add(StaffShift entity);
        void Update(StaffShift entity);
        void Delete(int id);
    }
}
