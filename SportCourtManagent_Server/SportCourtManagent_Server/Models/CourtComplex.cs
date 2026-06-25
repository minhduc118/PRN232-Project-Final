using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("CourtComplexes")]
    public class CourtComplex
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComplexId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ComplexName { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = null!;

        [Required]
        public int ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public User Manager { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Court> Courts { get; set; } = new List<Court>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}

