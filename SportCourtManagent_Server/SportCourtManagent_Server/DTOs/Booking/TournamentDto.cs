using System;
using System.Collections.Generic;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DTOs.Booking
{
  public class TournamentDto
  {
    public int TournamentId { get; set; }
    public string TournamentName { get; set; } = null!;
    public string? Description { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public TournamentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public List<BookingDto> Bookings { get; set; } = new List<BookingDto>();
  }
}
