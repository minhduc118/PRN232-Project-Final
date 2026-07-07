namespace SportCourtManagent_Server.DTOs.Bookings
{
    public class BookingServiceResponseDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
