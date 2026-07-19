using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Booking
{
  /// <summary>Request DTO for Customer to update a tournament within 24h (name, description, courts, slots, services).</summary>
  public class UpdateTournamentInfoRequest
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
