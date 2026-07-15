using System.Linq;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Services.Implements
{
    public static class UserMapper
    {
        public static UserDto ToSummaryDto(User user)
        {
            var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";
            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Role = roleName,
                MembershipTierName = user.MembershipTier?.TierName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public static UserDto ToFullDto(User user)
        {
            var dto = ToSummaryDto(user);
            dto.DateOfBirth = user.DateOfBirth;
            dto.LoyaltyPoints = user.LoyaltyPoints;
            dto.MembershipTierId = user.MembershipTierId;
            dto.Gender = user.Gender.ToString();
            dto.SkillLevel = user.SkillLevel.ToString();
            return dto;
        }
    }
}
