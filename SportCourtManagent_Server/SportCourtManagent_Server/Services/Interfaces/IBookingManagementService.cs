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

    /// <summary>Creates recurring bookings across multiple weeks. Skips conflicting dates.</summary>
    Task<RecurringBookingResponseDto> CreateRecurringBookingAsync(int userId, CreateRecurringBookingRequest request);

    /// <summary>Updates booking status asynchronous.</summary>
    Task<BookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusRequest request);

    /// <summary>Creates a new tournament booking asynchronous.</summary>
    Task<TournamentDto> CreateTournamentAsync(int userId, CreateTournamentRequest request);

    /// <summary>Gets list of tournaments belonging to a specific customer.</summary>
    Task<IEnumerable<TournamentDto>> GetCustomerTournamentsAsync(int userId);

    /// <summary>Gets all tournaments with optional filters for admin and staff.</summary>
    Task<IEnumerable<TournamentDto>> GetAdminTournamentsAsync(DateTime? date, string? status);

    /// <summary>Gets full tournament detail. Customer can only view their own; Admin/Staff can view all.</summary>
    Task<TournamentDto?> GetTournamentDetailAsync(int tournamentId, int userId, bool isAdminOrStaff);

    /// <summary>Updates tournament status (Admin/Staff only). Cascades to child bookings on Cancel/Paid.</summary>
    Task<TournamentDto?> UpdateTournamentStatusAsync(int tournamentId, UpdateTournamentStatusRequest request);

    /// <summary>Updates tournament name and description (Customer only, within 24h of creation).</summary>
    Task<TournamentDto?> UpdateTournamentInfoAsync(int tournamentId, int userId, UpdateTournamentInfoRequest request);
  }
}
