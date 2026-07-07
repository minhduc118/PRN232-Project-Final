using System;
using System.Collections.Generic;

namespace SportCourtManagent_Server.DTOs.Bookings
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = null!;
        public int UserId { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; } = null!;
        public int SlotId { get; set; }
        public string SlotName { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? PromoCode { get; set; }
        public List<BookingServiceResponseDto> BookingServices { get; set; } = new List<BookingServiceResponseDto>();
    }
}
