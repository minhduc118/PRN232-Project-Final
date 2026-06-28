using System;

namespace SportCourtManagent_Server.DTOs.Customer
{
    public class CustomerDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public int LoyaltyPoints { get; set; }
        public int? MembershipTierId { get; set; }
        public string? MembershipTierName { get; set; }
        public bool IsActive { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string SkillLevel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
