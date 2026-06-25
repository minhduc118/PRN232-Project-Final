using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("CoachSchedules")]
    public class CoachSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleId { get; set; }

        [Required]
        public int CoachId { get; set; }

        [ForeignKey("CoachId")]
        public User Coach { get; set; } = null!;

        [Required]
        public int CourtId { get; set; }

        [ForeignKey("CourtId")]
        public Court Court { get; set; } = null!;

        [Required]
        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        public TimeSlot TimeSlot { get; set; } = null!;

        [Required]
        public DateTime ScheduleDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsBooked { get; set; } = false;
    }
}

