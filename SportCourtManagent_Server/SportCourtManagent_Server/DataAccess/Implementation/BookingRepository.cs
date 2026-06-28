using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
  public class BookingRepository : IBookingRepository
  {
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>Gets customer bookings asynchronous.</summary>
    public async Task<IEnumerable<Booking>> GetCustomerBookingsAsync(int userId)
    {
      return await _context.Bookings
        .Include(b => b.Court)
        .Include(b => b.TimeSlot)
        .Include(b => b.Payment)
        .Include(b => b.Promotion)
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.BookingDate)
        .ToListAsync();
    }

    /// <summary>Gets admin bookings with optional filters asynchronous.</summary>
    public async Task<IEnumerable<Booking>> GetAdminBookingsAsync(DateTime? date, int? courtTypeId, string? status)
    {
      var query = _context.Bookings
        .Include(b => b.User)
        .Include(b => b.Court)
        .Include(b => b.TimeSlot)
        .Include(b => b.Payment)
        .Include(b => b.Promotion)
        .AsQueryable();

      if (date.HasValue)
      {
        query = query.Where(b => b.BookingDate.Date == date.Value.Date);
      }
      if (courtTypeId.HasValue)
      {
        query = query.Where(b => b.Court.CourtTypeId == courtTypeId.Value);
      }
      if (!string.IsNullOrEmpty(status) && Enum.TryParse<Enums.BookingStatus>(status, true, out var st))
      {
        query = query.Where(b => b.Status == st);
      }

      return await query.OrderByDescending(b => b.BookingDate).ToListAsync();
    }

    /// <summary>Gets booking detail including related entities asynchronous.</summary>
    public async Task<Booking?> GetDetailAsync(int id)
    {
      return await _context.Bookings
        .Include(b => b.User)
        .Include(b => b.Court)
        .Include(b => b.TimeSlot)
        .Include(b => b.Payment)
        .Include(b => b.Promotion)
        .Include(b => b.BookingServices)
          .ThenInclude(bs => bs.Service)
        .FirstOrDefaultAsync(b => b.BookingId == id);
    }

    /// <summary>Adds a new booking asynchronous.</summary>
    public async Task AddAsync(Booking entity)
    {
      if (entity == null) throw new ArgumentNullException(nameof(entity));
      await _context.Bookings.AddAsync(entity);
      await _context.SaveChangesAsync();
    }

    /// <summary>Updates an existing booking asynchronous.</summary>
    public async Task UpdateAsync(Booking entity)
    {
      if (entity == null) throw new ArgumentNullException(nameof(entity));
      _context.Bookings.Update(entity);
      await _context.SaveChangesAsync();
    }
  }
}
