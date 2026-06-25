using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IRecurringBookingRepository
    {
        IEnumerable<RecurringBooking> GetAll();
        RecurringBooking? GetById(int id);
        void Add(RecurringBooking entity);
        void Update(RecurringBooking entity);
        void Delete(int id);
    }
}
