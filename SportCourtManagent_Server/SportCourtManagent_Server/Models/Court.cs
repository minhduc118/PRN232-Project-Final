using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("Courts")]
    public class Court
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourtId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CourtName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string CourtCode { get; set; } = null!;

        [Required]
        public int CourtTypeId { get; set; }

        [ForeignKey("CourtTypeId")]
        public CourtType CourtType { get; set; } = null!;

        [Required]
        public int ComplexId { get; set; }

        [ForeignKey("ComplexId")]
        public CourtComplex Complex { get; set; } = null!;

        [Required]
        public CourtStatus Status { get; set; } = CourtStatus.Available;

        [Required]
        public TimeSpan OpenTime { get; set; }

        [Required]
        public TimeSpan CloseTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerHour { get; set; }

        [MaxLength(50)]
        public string? CourtSize { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<CourtImage> CourtImages { get; set; } = new List<CourtImage>();
        public ICollection<CourtPricing> CourtPricings { get; set; } = new List<CourtPricing>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<CoachSchedule> CoachSchedules { get; set; } = new List<CoachSchedule>();
        public ICollection<RecurringBooking> RecurringBookings { get; set; } = new List<RecurringBooking>();
        public ICollection<Waitlist> Waitlists { get; set; } = new List<Waitlist>();
        public ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
    }
}

