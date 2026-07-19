using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
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
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IMembershipTierRepository tierRepo,
            IJwtTokenService jwtTokenService,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _tierRepo = tierRepo;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
        }

        public async Task<string?> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepo.ExistsByEmailAsync(request.Email))
                return "Email này đã được đăng ký.";

            if (!string.IsNullOrWhiteSpace(request.Phone) && await _userRepo.ExistsByPhoneAsync(request.Phone))
                return "Số điện thoại này đã được đăng ký.";

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
            var cleanEmail = request.Email?.Trim() ?? string.Empty;
            var cleanPassword = request.Password?.Trim() ?? string.Empty;

            var user = await _userRepo.GetByEmailWithDetailsAsync(cleanEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(cleanPassword, user.PasswordHash))
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

        public async Task<(AuthResponseDto? Data, string? Error)> GoogleLoginAsync(GoogleLoginRequest request)
        {
            var googleSettings = _configuration.GetSection("GoogleSettings");
            var clientId = googleSettings["ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                return (null, "Chưa cấu hình Google ClientID trên server.");
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { clientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (Exception ex)
            {
                return (null, $"Token Google không hợp lệ hoặc đã hết hạn: {ex.Message}");
            }

            var email = payload.Email;
            var fullName = payload.Name ?? "Google User";
            var avatarUrl = payload.Picture;

            var user = await _userRepo.GetByEmailWithDetailsAsync(email);

            if (user != null)
            {
                if (!user.IsActive)
                    return (null, "Tài khoản của bạn đã bị khóa.");

                // Đọc vai trò hiện tại của tài khoản
                var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";

                // Chỉ các tk đã đc admin set (Admin, Manager, Staff, Coach) mới đăng nhập bằng gg với vai trò đó,
                // còn lại (Customer hoặc vai trò khác) mặc định là customer.
                var allowedRoles = new HashSet<string> { "Admin", "Manager", "Staff", "Coach" };
                if (!allowedRoles.Contains(roleName))
                {
                    roleName = "Customer";

                    // Đồng bộ vai trò Customer vào DB nếu chưa có
                    var customerRole = await _roleRepo.GetByNameAsync("Customer");
                    if (customerRole != null && !user.UserRoles.Any(ur => ur.RoleId == customerRole.RoleId))
                    {
                        await _userRoleRepo.ReplaceUserRoleAsync(user.UserId, customerRole.RoleId);
                        // Re-fetch user để tải lại thông tin role mới
                        user = await _userRepo.GetByEmailWithDetailsAsync(email) ?? user;
                    }
                }

                var accessToken = _jwtTokenService.GenerateToken(user, roleName);
                var refreshToken = Guid.NewGuid().ToString();

                user.RefreshToken = refreshToken;
                if (!string.IsNullOrEmpty(avatarUrl) && string.IsNullOrEmpty(user.AvatarUrl))
                {
                    user.AvatarUrl = avatarUrl;
                }
                await _userRepo.UpdateAsync(user);

                return (new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = UserMapper.ToFullDto(user)
                }, null);
            }
            else
            {
                // Người dùng chưa có tài khoản - Tự động đăng ký làm Customer
                var defaultTier = await _tierRepo.GetByNameAsync("Bronze")
                    ?? await _tierRepo.GetFirstAsync();

                var newUser = new User
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Password ngẫu nhiên
                    IsActive = true,
                    MembershipTierId = defaultTier?.TierId,
                    Gender = Gender.Other,
                    SkillLevel = SkillLevel.Beginner,
                    AvatarUrl = avatarUrl,
                    CreatedAt = DateTime.Now
                };

                await _userRepo.AddAsync(newUser);

                var customerRole = await _roleRepo.GetByNameAsync("Customer");
                if (customerRole != null)
                {
                    await _userRoleRepo.AddAsync(new UserRole
                    {
                        UserId = newUser.UserId,
                        RoleId = customerRole.RoleId
                    });
                }

                // Tải lại người dùng kèm theo chi tiết UserRoles/Role để tạo Token chính xác
                var registeredUser = await _userRepo.GetByEmailWithDetailsAsync(email) ?? newUser;

                var accessToken = _jwtTokenService.GenerateToken(registeredUser, "Customer");
                var refreshToken = Guid.NewGuid().ToString();

                registeredUser.RefreshToken = refreshToken;
                await _userRepo.UpdateAsync(registeredUser);

                return (new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = UserMapper.ToFullDto(registeredUser)
                }, null);
            }
        }
    }
}
