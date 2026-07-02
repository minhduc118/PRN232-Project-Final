using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs.Auth;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Helpers;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email này đã được đăng ký." });
            }

            var defaultTier = await _context.MembershipTiers
                .FirstOrDefaultAsync(t => t.TierName == "Bronze")
                ?? await _context.MembershipTiers.FirstOrDefaultAsync();

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

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            if (customerRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = customerRole.RoleId
                };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đăng ký tài khoản thành công." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.MembershipTier)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "Email hoặc mật khẩu không chính xác." });
            }

            if (!user.IsActive)
            {
                return BadRequest(new { message = "Tài khoản của bạn đã bị khóa." });
            }

            var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";
            var accessToken = GenerateJwtToken(user, roleName);
            var refreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    AvatarUrl = user.AvatarUrl,
                    DateOfBirth = user.DateOfBirth,
                    LoyaltyPoints = user.LoyaltyPoints,
                    MembershipTierId = user.MembershipTierId,
                    MembershipTierName = user.MembershipTier?.TierName ?? "Bronze",
                    Role = roleName,
                    Gender = user.Gender.ToString(),
                    SkillLevel = user.SkillLevel.ToString(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(ApiResults.Ok(response, "Đăng nhập thành công."));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResults.Fail("Không xác định được người dùng.", 401));
            }

            var user = await _context.Users
                .Include(u => u.MembershipTier)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(ApiResults.Fail("Người dùng không tồn tại.", 404));
            }

            var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";

            var userDto = new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                LoyaltyPoints = user.LoyaltyPoints,
                MembershipTierId = user.MembershipTierId,
                MembershipTierName = user.MembershipTier?.TierName ?? "Bronze",
                Role = roleName,
                Gender = user.Gender.ToString(),
                SkillLevel = user.SkillLevel.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(ApiResults.Ok(userDto));
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-min-32-chars-long-sports-court!!";
            var issuer = jwtSettings["Issuer"] ?? "SportCourtManagent_Server";
            var audience = jwtSettings["Audience"] ?? "SportCourtClient";
            var exprMinutes = double.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "120");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("FullName", user.FullName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(exprMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
