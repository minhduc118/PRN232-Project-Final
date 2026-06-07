using SportCourtManagerment.DTOs.Bookings;
using SportCourtManagerment.DTOs;

namespace SportCourtManagerment.Services.Bookings;

public interface IBookingService
{
    Task<IEnumerable<BookingAdminDto>> GetAdminBookingsAsync(DateOnly? date, int? courtTypeId, string? status);
    Task<BookingAdminDto?> GetBookingByIdAsync(int id);
    Task<ApiResponse<BookingAdminDto>> CreateBookingFromAdminAsync(CreateBookingAdminDto dto);
    Task<ApiResponse<BookingAdminDto>> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto);
}
