using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("PlayerRequests")]
    public class PlayerRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } = null!;

        [Required]
        public int HostUserId { get; set; }

        [ForeignKey("HostUserId")]
        public User HostUser { get; set; } = null!;

        [Required]
        public SkillLevel SkillLevel { get; set; } = SkillLevel.Beginner;

        [Required]
        public int RequiredPlayers { get; set; }

        [Required]
        public Gender GenderPref { get; set; } = Gender.Other;

        [Required]
        public PlayerRequestStatus Status { get; set; } = PlayerRequestStatus.Open;

        public ICollection<PlayerRequestMember> PlayerRequestMembers { get; set; } = new List<PlayerRequestMember>();
    }
}

