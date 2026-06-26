using SportCourtManagerment.DTOs.Request.Bookings;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Repository.Bookings
{
    public interface IBookingService
    {
        Task<bool> CreateBookingAsync(CreateBookingRequestDTO requestDTO, int userId);
    }
}
