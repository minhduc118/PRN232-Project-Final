using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingCode { get; set; } = null!;

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
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public int? PromotionId { get; set; }

        [ForeignKey("PromotionId")]
        public Promotion? Promotion { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        public int? TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public Tournament? Tournament { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public Payment? Payment { get; set; }
        public Review? Review { get; set; }
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<PlayerRequest> PlayerRequests { get; set; } = new List<PlayerRequest>();
        public Invoice? Invoice { get; set; }
    }
}

