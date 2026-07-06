using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Booking
{
  public class CreateRecurringBookingRequest
  {
    [Required]
    public int CourtId { get; set; }

    [Required]
    public int SlotId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>Days of week: 0=Sunday, 1=Monday, 2=Tuesday, ..., 6=Saturday</summary>
    [Required]
    public List<int> DaysOfWeek { get; set; } = new();

    public string? PromotionCode { get; set; }
    public string? Note { get; set; }
  }
}
