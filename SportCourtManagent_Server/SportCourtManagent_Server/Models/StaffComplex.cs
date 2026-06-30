using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    /// <summary>
    /// Junction table: ánh xạ Staff (User) với CourtComplex.
    /// Một Staff có thể được assign vào nhiều Complex, một Complex có nhiều Staff.
    /// </summary>
    [Table("StaffComplexes")]
    public class StaffComplex
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StaffComplexId { get; set; }

        [Required]
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public User Staff { get; set; } = null!;

        [Required]
        public int ComplexId { get; set; }

        [ForeignKey("ComplexId")]
        public CourtComplex Complex { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}
