using System;
using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IInMemoryBookingRepository
    {
        Task SaveAsync(Booking booking, TimeSpan expiration);
        Task<Booking?> GetByCodeAsync(string bookingCode);
        Task<IEnumerable<Booking>> GetAllPendingAsync();
        Task RemoveAsync(string bookingCode);
        Task<bool> HasConflictingBookingAsync(int courtId, int slotId, DateTime bookingDate);
    }
}
