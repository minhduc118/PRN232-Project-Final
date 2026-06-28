using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IBookingServiceRepository
    {
        IEnumerable<BookingService> GetAll();
        BookingService? GetById(int id);
        void Add(BookingService entity);
        void Update(BookingService entity);
        void Delete(int id);
    }
}
