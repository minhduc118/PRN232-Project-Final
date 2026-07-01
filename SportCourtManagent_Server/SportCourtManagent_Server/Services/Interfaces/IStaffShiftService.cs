using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IStaffShiftService
    {
        IEnumerable<StaffShift> GetTodayShifts();
        StaffShift? CheckIn(int shiftId, string photoBase64);
        StaffShift? CheckOut(int shiftId, string photoBase64);
        void SeedDemoData();
    }
}
