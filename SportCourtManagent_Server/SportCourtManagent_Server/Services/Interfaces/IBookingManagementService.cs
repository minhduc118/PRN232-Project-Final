using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Booking;

namespace SportCourtManagent_Server.Services.Interfaces
{
  public interface IBookingManagementService
  {
    Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(int userId);
    Task<PagedResult<BookingDto>> GetPagedCustomerBookingsAsync(int userId, BookingFilterParams filter);

    Task<IEnumerable<BookingDto>> GetAdminBookingsAsync(DateTime? date, int? courtTypeId, string? status);
    Task<PagedResult<BookingDto>> GetPagedAdminBookingsAsync(BookingFilterParams filter);

    Task<BookingDto?> GetBookingDetailAsync(int id);
    Task<BookingDto> CreateBookingAsync(int userId, CreateBookingRequest request);
    /// <summary>Creates recurring bookings across multiple weeks. Skips conflicting dates.</summary>
    Task<RecurringBookingResponseDto> CreateRecurringBookingAsync(int userId, CreateRecurringBookingRequest request);

    /// <summary>Updates booking status asynchronous.</summary>
    Task<BookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusRequest request);
    Task<TournamentDto> CreateTournamentAsync(int userId, CreateTournamentRequest request);

    Task<IEnumerable<TournamentDto>> GetCustomerTournamentsAsync(int userId);
    Task<PagedResult<TournamentDto>> GetPagedCustomerTournamentsAsync(int userId, TournamentFilterParams filter);

    Task<IEnumerable<TournamentDto>> GetAdminTournamentsAsync(DateTime? date, string? status);
    Task<PagedResult<TournamentDto>> GetPagedAdminTournamentsAsync(TournamentFilterParams filter);

    Task<PagedResult<TournamentPublicDto>> GetPagedPublicTournamentsAsync(TournamentFilterParams filter);

    Task<TournamentDto?> GetTournamentDetailAsync(int tournamentId, int userId, bool isAdminOrStaff);
    Task<TournamentDto?> UpdateTournamentStatusAsync(int tournamentId, UpdateTournamentStatusRequest request);
    Task<TournamentPublicDto?> GetTournamentPublicInfoAsync(int tournamentId);

    /// <summary>Updates tournament name and description (Customer only, within 24h of creation).</summary>
    Task<TournamentDto?> UpdateTournamentInfoAsync(int tournamentId, int userId, UpdateTournamentInfoRequest request);
  }
}
