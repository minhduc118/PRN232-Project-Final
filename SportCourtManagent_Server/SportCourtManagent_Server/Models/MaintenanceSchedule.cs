using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("MaintenanceSchedules")]
    public class MaintenanceSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaintenanceId { get; set; }

        [Required]
        public int CourtId { get; set; }

        [ForeignKey("CourtId")]
        public Court Court { get; set; } = null!;

        [Required]
        public MaintenanceType MaintenanceType { get; set; } = MaintenanceType.Routine;

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public int? AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public User? AssignedStaff { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Result { get; set; }

        [MaxLength(500)]
        public string? ImageProof { get; set; }

        [Required]
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;
    }
}


