using System;
using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Booking
{
    public class CreateWaitlistRequest
    {
        [Required]
        public int CourtId { get; set; }

        [Required]
        public int SlotId { get; set; }

        [Required]
        public DateTime WaitDate { get; set; }
    }

    public class WaitlistResponseDto
    {
        public int WaitlistId { get; set; }
        public int UserId { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; } = null!;
        public int SlotId { get; set; }
        public string SlotName { get; set; } = null!;
        public DateTime WaitDate { get; set; }
        public int Position { get; set; }
        public string Status { get; set; } = null!;
    }
}
