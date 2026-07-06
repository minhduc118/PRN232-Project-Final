using System;
using System.Collections.Generic;

namespace SportCourtManagent_Server.DTOs.Booking
{
  public class RecurringBookingResponseDto
  {
    public int RecurringId { get; set; }
    public int CourtId { get; set; }
    public string CourtName { get; set; } = null!;
    public int SlotId { get; set; }
    public string SlotName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DaysOfWeek { get; set; } = null!;
    public string Status { get; set; } = null!;

    /// <summary>List of bookings successfully created.</summary>
    public List<BookingDto> CreatedBookings { get; set; } = new();

    /// <summary>List of dates that had conflicts (already booked by someone else).</summary>
    public List<string> ConflictDates { get; set; } = new();

    /// <summary>Total number of sessions requested.</summary>
    public int TotalRequestedSessions { get; set; }

    /// <summary>Total number of sessions successfully booked.</summary>
    public int TotalBookedSessions { get; set; }

    /// <summary>Total estimated amount for all booked sessions.</summary>
    public decimal TotalEstimatedAmount { get; set; }

    /// <summary>Whether there were any date conflicts.</summary>
    public bool HasConflicts => ConflictDates.Count > 0;
  }
}
