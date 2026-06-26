using SportCourtManagerment.Models;

namespace SportCourtManagerment.DTOs.Request.Bookings
{
    public class CreateBookingRequestDTO
    {
        public required int CourtId { get; set; }
        public required int[] TimeSlotIds { get; set; }
        public String PromotionCode { get; set; } = string.Empty;
        public String Note { get; set; } = string.Empty;
    }
}
