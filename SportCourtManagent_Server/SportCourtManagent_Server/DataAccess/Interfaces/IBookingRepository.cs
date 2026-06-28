using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
  public interface IBookingRepository
  {
    /// <summary>Gets customer bookings asynchronous.</summary>
    Task<IEnumerable<Booking>> GetCustomerBookingsAsync(int userId);

    /// <summary>Gets admin bookings with optional filters asynchronous.</summary>
    Task<IEnumerable<Booking>> GetAdminBookingsAsync(DateTime? date, int? courtTypeId, string? status);

    /// <summary>Gets booking detail including related entities asynchronous.</summary>
    Task<Booking?> GetDetailAsync(int id);

    /// <summary>Adds a new booking asynchronous.</summary>
    Task AddAsync(Booking entity);

    /// <summary>Updates an existing booking asynchronous.</summary>
    Task UpdateAsync(Booking entity);
  }
}
