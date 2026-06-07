using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.DTOs.Payments;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface ISePayService
    {
        SePayQrCodeResponse GetQrCode(string bookingCode);
        Task<BookingResponseDto> HandleWebhookAsync(SePayWebhookPayload payload, string? authHeader);
    }
}
