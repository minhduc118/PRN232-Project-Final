using System.ComponentModel.DataAnnotations;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DTOs.Booking
{
  public class UpdateBookingStatusRequest
  {
    [Required]
    public BookingStatus Status { get; set; }

    public string? CancelReason { get; set; }
  }
}
