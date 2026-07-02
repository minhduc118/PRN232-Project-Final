using System.ComponentModel.DataAnnotations;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DTOs.Booking
{
  /// <summary>Request DTO for Admin/Staff to update tournament status.</summary>
  public class UpdateTournamentStatusRequest
  {
    [Required]
    public TournamentStatus Status { get; set; }

    public string? CancelReason { get; set; }
  }
}
