using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.DTOs.Bookings;

public class UpdateBookingStatusDto
{
    [Required]
    public BookingStatus Status { get; set; }

    public string? CancelReason { get; set; }
}
