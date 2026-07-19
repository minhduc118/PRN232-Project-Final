using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Booking
{
  public class CourtSlotSelection
  {
    [Required]
    public int CourtId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public List<int> SlotIds { get; set; } = new List<int>();

    public List<ServiceItemRequest>? Services { get; set; }
  }

  public class CreateTournamentRequest
  {
    [Required]
    [MaxLength(200)]
    public string TournamentName { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public List<CourtSlotSelection> CourtSelections { get; set; } = new List<CourtSlotSelection>();

    public string? PromotionCode { get; set; }
    public string? Note { get; set; }
  }
}
