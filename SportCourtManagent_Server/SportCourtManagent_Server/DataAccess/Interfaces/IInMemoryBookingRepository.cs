using System;
using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IInMemoryBookingRepository
    {
        void Save(Booking booking, TimeSpan expiration);
        Booking? GetByCode(string bookingCode);
        IEnumerable<Booking> GetAllPending();
        void Remove(string bookingCode);
        bool HasConflictingBooking(int courtId, int slotId, DateTime bookingDate);
    }
}
