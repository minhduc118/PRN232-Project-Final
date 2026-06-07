using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Bookings
{
    public class ConfirmPaymentRequestDto
    {
        [Required(ErrorMessage = "Booking code is required.")]
        public string BookingCode { get; set; } = null!;

        [Required(ErrorMessage = "Payment method is required.")]
        public string PaymentMethod { get; set; } = null!;

        [Required(ErrorMessage = "Transaction ID is required.")]
        [StringLength(200, ErrorMessage = "Transaction ID cannot exceed 200 characters.")]
        public string TransactionId { get; set; } = null!;
    }
}
