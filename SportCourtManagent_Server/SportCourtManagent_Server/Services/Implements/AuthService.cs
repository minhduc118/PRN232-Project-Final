using System;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Auth;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IMembershipTierRepository _tierRepo;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IMembershipTierRepository tierRepo,
            IJwtTokenService jwtTokenService)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _tierRepo = tierRepo;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string?> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepo.ExistsByEmailAsync(request.Email))
                return "Email này đã được đăng ký.";

            var defaultTier = await _tierRepo.GetByNameAsync("Bronze")
                ?? await _tierRepo.GetFirstAsync();

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                MembershipTierId = defaultTier?.TierId,
                Gender = Gender.Other,
                SkillLevel = SkillLevel.Beginner,
                CreatedAt = DateTime.Now
            };

            await _userRepo.AddAsync(user);

            var customerRole = await _roleRepo.GetByNameAsync("Customer");
            if (customerRole != null)
            {
                await _userRoleRepo.AddAsync(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = customerRole.RoleId
                });
            }

            return null;
        }

        public async Task<(AuthResponseDto? Data, string? Error)> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByEmailWithDetailsAsync(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return (null, "Email hoặc mật khẩu không chính xác.");

            if (!user.IsActive)
                return (null, "Tài khoản của bạn đã bị khóa.");

            var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";
            var accessToken = _jwtTokenService.GenerateToken(user, roleName);
            var refreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            await _userRepo.UpdateAsync(user);

            return (new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = UserMapper.ToFullDto(user)
            }, null);
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(userId);
            return user == null ? null : UserMapper.ToFullDto(user);
        }
    }
}
