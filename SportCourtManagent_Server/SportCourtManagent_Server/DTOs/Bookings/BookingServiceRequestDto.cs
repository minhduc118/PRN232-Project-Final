using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Bookings
{
    public class BookingServiceRequestDto
    {
        [Required(ErrorMessage = "Service ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Service ID must be a positive integer.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
