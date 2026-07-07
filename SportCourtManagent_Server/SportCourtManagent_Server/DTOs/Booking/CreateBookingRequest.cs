using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Booking
{
  public class ServiceItemRequest
  {
    [Range(1, int.MaxValue, ErrorMessage = "Service ID must be valid")]
    public int ServiceId { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; }
  }

  public class CreateBookingRequest
  {
    [Required]
    public int CourtId { get; set; }

    [Required]
    public int SlotId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    public List<ServiceItemRequest>? ServiceIds { get; set; }
    public string? PromotionCode { get; set; }
    public string? Note { get; set; }

    public bool? IsRecurring { get; set; }
    public List<int>? RecurringDays { get; set; }
    public DateTime? RecurringEndDate { get; set; }
  }
}
