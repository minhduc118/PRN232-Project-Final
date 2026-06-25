using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportCourtManagent_Server.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = null!;

        [MaxLength(100)]
        public string? TableName { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? Details { get; set; }
    }
}

