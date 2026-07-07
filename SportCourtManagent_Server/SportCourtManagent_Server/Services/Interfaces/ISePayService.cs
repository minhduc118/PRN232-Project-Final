using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.DTOs.Payments;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface ISePayService
    {
        Task<SePayQrCodeResponse> GetQrCodeAsync(string bookingCode);
        Task<BookingResponseDto> HandleWebhookAsync(SePayWebhookPayload payload, string? authHeader);
    }
}
