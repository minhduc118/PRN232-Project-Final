using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.DTOs.Bookings;

public class CreateBookingAdminDto
{
    [Required]
    public int UserId { get; set; } // Admin can create for an existing user

    [Required]
    public int CourtId { get; set; }

    [Required]
    public int SlotId { get; set; }

    [Required]
    public DateOnly BookingDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    public string? PromotionCode { get; set; }

    public string? Note { get; set; }

    [Required]
    public string PaymentStatus { get; set; } = "Pending"; // "Pending", "Success"
    
    public string? PaymentMethod { get; set; } // "Cash", "BankTransfer"
}
