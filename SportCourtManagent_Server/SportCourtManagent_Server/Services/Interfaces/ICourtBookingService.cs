using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Bookings;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface ICourtBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto dto, int userId);
        Task<BookingResponseDto> ConfirmPaymentAsync(ConfirmPaymentRequestDto dto);
    }
}
