using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("PlayerRequestMembers")]
    public class PlayerRequestMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlayerRequestMemberId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [ForeignKey("RequestId")]
        public PlayerRequest PlayerRequest { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public MemberRequestStatus Status { get; set; } = MemberRequestStatus.Pending;

        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}

