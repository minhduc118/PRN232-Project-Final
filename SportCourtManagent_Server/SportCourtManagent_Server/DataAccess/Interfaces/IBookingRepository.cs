using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking entity);
        Task UpdateAsync(Booking entity);
        Task DeleteAsync(int id);
        Task<bool> HasConflictingBookingAsync(int courtId, int slotId, DateTime bookingDate);
        Task<BookingBillingResult> ProcessBookingBillingAsync(CreateBookingRequestDto dto, decimal courtPrice);
    }
}
