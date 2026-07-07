using System;
using System.Collections.Generic;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DTOs.Booking
{
  /// <summary>Court slot summary for public tournament view (no price or payment info).</summary>
  public class CourtSlotPublicDto
  {
    public int CourtId { get; set; }
    public string CourtName { get; set; } = null!;
    public int SlotId { get; set; }
    public string SlotName { get; set; } = null!;
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; }
  }

  /// <summary>Public tournament info DTO — visible to any authenticated customer, hides sensitive data.</summary>
  public class TournamentPublicDto
  {
    public int TournamentId { get; set; }
    public string TournamentName { get; set; } = null!;
    public string? Description { get; set; }
    public string OrganizerName { get; set; } = null!;
    public TournamentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CourtSlotPublicDto> Courts { get; set; } = new List<CourtSlotPublicDto>();
  }
}
