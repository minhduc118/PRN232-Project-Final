using SportCourtManagent_Server.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("Tasks")]
    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public TaskType TaskType { get; set; } = TaskType.Manual;

        [Required]
        public TaskCategory Category { get; set; } = TaskCategory.Cleanup;

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Required]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

        [Required]
        public int ComplexId { get; set; }

        [ForeignKey("ComplexId")]
        public CourtComplex Complex { get; set; } = null!;

        public int? AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public User? AssignedStaff { get; set; }

        public int? CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public User? CreatedBy { get; set; }

        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string? ImageProof { get; set; }
    }
}


