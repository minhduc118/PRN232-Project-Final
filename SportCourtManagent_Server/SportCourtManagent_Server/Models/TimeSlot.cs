using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("TimeSlots")]
    public class TimeSlot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SlotId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SlotName { get; set; } = null!;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public DayType DayType { get; set; } = DayType.Weekday;

        public ICollection<CourtPricing> CourtPricings { get; set; } = new List<CourtPricing>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<RecurringBooking> RecurringBookings { get; set; } = new List<RecurringBooking>();
        public ICollection<Waitlist> Waitlists { get; set; } = new List<Waitlist>();
        public ICollection<CoachSchedule> CoachSchedules { get; set; } = new List<CoachSchedule>();
    }
}

