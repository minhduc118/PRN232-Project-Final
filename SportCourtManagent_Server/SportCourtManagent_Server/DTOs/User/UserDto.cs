using System;

namespace SportCourtManagent_Server.DTOs.User
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int LoyaltyPoints { get; set; }
        public int? MembershipTierId { get; set; }
        public string? MembershipTierName { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string SkillLevel { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
