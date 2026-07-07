using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetCustomerBookingsAsync(int userId);
        Task<IEnumerable<Booking>> GetAdminBookingsAsync(DateTime? date, int? courtTypeId, string? status);
        Task<Booking?> GetDetailAsync(int id);
        Task AddAsync(Booking entity);
        Task UpdateAsync(Booking entity);
        Task DeleteAsync(int id);
        Task<bool> HasConflictingBookingAsync(int courtId, int slotId, DateTime bookingDate);
        Task<BookingBillingResult> ProcessBookingBillingAsync(CreateBookingRequestDto dto, decimal courtPrice);
    }
}
