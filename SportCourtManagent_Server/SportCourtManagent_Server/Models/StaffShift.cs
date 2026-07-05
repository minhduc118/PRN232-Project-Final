using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("StaffShifts")]
    public class StaffShift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShiftId { get; set; }

        [Required]
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public User Staff { get; set; } = null!;

        [Required]
        public int ComplexId { get; set; }

        [ForeignKey("ComplexId")]
        public CourtComplex Complex { get; set; } = null!;

        [Required]
        public DateOnly ShiftDate { get; set; }

        [Required]
        public ShiftType ShiftType { get; set; } = ShiftType.Morning;

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }
    }
}

