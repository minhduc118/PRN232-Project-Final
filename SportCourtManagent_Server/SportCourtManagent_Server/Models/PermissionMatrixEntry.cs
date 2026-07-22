using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("PermissionMatrix")]
    public class PermissionMatrixEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Feature { get; set; } = null!;

        public bool Admin { get; set; }
        public bool Manager { get; set; }
        public bool Staff { get; set; }
        public bool Customer { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
