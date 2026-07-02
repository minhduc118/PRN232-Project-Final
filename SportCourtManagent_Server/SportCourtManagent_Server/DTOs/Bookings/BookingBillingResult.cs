using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DTOs.Bookings
{
    public class BookingBillingResult
    {
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public Models.Promotion? AppliedPromotion { get; set; }
    }
}
