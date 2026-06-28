using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ITimeSlotRepository
    {
        IEnumerable<TimeSlot> GetAll();
        TimeSlot? GetById(int id);
        void Add(TimeSlot entity);
        void Update(TimeSlot entity);
        void Delete(int id);
    }
}
