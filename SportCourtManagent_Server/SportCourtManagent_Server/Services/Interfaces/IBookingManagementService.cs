using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Booking;

namespace SportCourtManagent_Server.Services.Interfaces
{
  public interface IBookingManagementService
  {
    /// <summary>Gets customer bookings asynchronous.</summary>
    Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(int userId);

    /// <summary>Gets admin bookings with filters asynchronous.</summary>
    Task<IEnumerable<BookingDto>> GetAdminBookingsAsync(DateTime? date, int? courtTypeId, string? status);

    /// <summary>Gets booking detail by id asynchronous.</summary>
    Task<BookingDto?> GetBookingDetailAsync(int id);

    /// <summary>Creates a new booking asynchronous.</summary>
    Task<BookingDto> CreateBookingAsync(int userId, CreateBookingRequest request);

    /// <summary>Updates booking status asynchronous.</summary>
    Task<BookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusRequest request);
  }
}
