using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("Waitlists")]
    public class Waitlist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WaitlistId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public int CourtId { get; set; }

        [ForeignKey("CourtId")]
        public Court Court { get; set; } = null!;

        [Required]
        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        public TimeSlot TimeSlot { get; set; } = null!;

        [Required]
        public DateTime WaitDate { get; set; }

        [Required]
        public int Position { get; set; }

        [Required]
        public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;

        public DateTime? NotifiedAt { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}

