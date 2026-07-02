using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs.Customer;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            if (customerRole == null)
            {
                return Ok(new { data = Array.Empty<CustomerDto>() });
            }

            var query = _context.Users
                .Include(u => u.MembershipTier)
                .Include(u => u.UserRoles)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == customerRole.RoleId));

            if (!string.IsNullOrEmpty(search))
            {
                var term = search.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(term) 
                                      || u.Email.ToLower().Contains(term) 
                                      || (u.Phone != null && u.Phone.Contains(term)));
            }

            var customers = await query
                .OrderBy(u => u.FullName)
                .Select(u => new CustomerDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    AvatarUrl = u.AvatarUrl,
                    LoyaltyPoints = u.LoyaltyPoints,
                    MembershipTierId = u.MembershipTierId,
                    MembershipTierName = u.MembershipTier != null ? u.MembershipTier.TierName : "Bronze",
                    IsActive = u.IsActive,
                    Gender = u.Gender.ToString(),
                    SkillLevel = u.SkillLevel.ToString(),
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new { data = customers });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var u = await _context.Users
                .Include(x => x.MembershipTier)
                .Include(x => x.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (u == null || !u.UserRoles.Any(ur => ur.Role.RoleName == "Customer"))
            {
                return NotFound(new { message = "Không tìm thấy khách hàng này." });
            }

            var dto = new CustomerDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                AvatarUrl = u.AvatarUrl,
                LoyaltyPoints = u.LoyaltyPoints,
                MembershipTierId = u.MembershipTierId,
                MembershipTierName = u.MembershipTier != null ? u.MembershipTier.TierName : "Bronze",
                IsActive = u.IsActive,
                Gender = u.Gender.ToString(),
                SkillLevel = u.SkillLevel.ToString(),
                CreatedAt = u.CreatedAt
            };

            return Ok(new { data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email này đã được sử dụng." });
            }

            if (!Enum.TryParse<Gender>(request.Gender, out var genderEnum))
            {
                return BadRequest(new { message = "Giới tính không hợp lệ." });
            }

            if (!Enum.TryParse<SkillLevel>(request.SkillLevel, out var skillEnum))
            {
                return BadRequest(new { message = "Trình độ không hợp lệ." });
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(string.IsNullOrEmpty(request.Password) ? "Customer@123" : request.Password),
                LoyaltyPoints = request.LoyaltyPoints,
                Gender = genderEnum,
                SkillLevel = skillEnum,
                IsActive = request.IsActive,
                CreatedAt = DateTime.Now
            };

            // Auto-assign tier
            await RecalculateMembershipTier(user);

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

            // Reload user for response DTO
            var createdUser = await _context.Users
                .Include(u => u.MembershipTier)
                .FirstAsync(u => u.UserId == user.UserId);

            var dto = new CustomerDto
            {
                UserId = createdUser.UserId,
                FullName = createdUser.FullName,
                Email = createdUser.Email,
                Phone = createdUser.Phone,
                AvatarUrl = createdUser.AvatarUrl,
                LoyaltyPoints = createdUser.LoyaltyPoints,
                MembershipTierId = createdUser.MembershipTierId,
                MembershipTierName = createdUser.MembershipTier?.TierName ?? "Bronze",
                IsActive = createdUser.IsActive,
                Gender = createdUser.Gender.ToString(),
                SkillLevel = createdUser.SkillLevel.ToString(),
                CreatedAt = createdUser.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, new { message = "Tạo khách hàng thành công.", data = dto });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null || !user.UserRoles.Any(ur => ur.Role.RoleName == "Customer"))
            {
                return NotFound(new { message = "Không tìm thấy khách hàng cần cập nhật." });
            }

            if (!Enum.TryParse<Gender>(request.Gender, out var genderEnum))
            {
                return BadRequest(new { message = "Giới tính không hợp lệ." });
            }

            if (!Enum.TryParse<SkillLevel>(request.SkillLevel, out var skillEnum))
            {
                return BadRequest(new { message = "Trình độ không hợp lệ." });
            }

            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.LoyaltyPoints = request.LoyaltyPoints;
            user.Gender = genderEnum;
            user.SkillLevel = skillEnum;
            user.IsActive = request.IsActive;

            // Recalculate tier based on updated loyalty points
            await RecalculateMembershipTier(user);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Fetch fully populated updated user
            var updatedUser = await _context.Users
                .Include(u => u.MembershipTier)
                .FirstAsync(u => u.UserId == user.UserId);

            var dto = new CustomerDto
            {
                UserId = updatedUser.UserId,
                FullName = updatedUser.FullName,
                Email = updatedUser.Email,
                Phone = updatedUser.Phone,
                AvatarUrl = updatedUser.AvatarUrl,
                LoyaltyPoints = updatedUser.LoyaltyPoints,
                MembershipTierId = updatedUser.MembershipTierId,
                MembershipTierName = updatedUser.MembershipTier?.TierName ?? "Bronze",
                IsActive = updatedUser.IsActive,
                Gender = updatedUser.Gender.ToString(),
                SkillLevel = updatedUser.SkillLevel.ToString(),
                CreatedAt = updatedUser.CreatedAt
            };

            return Ok(new { message = "Cập nhật khách hàng thành công.", data = dto });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null || !user.UserRoles.Any(ur => ur.Role.RoleName == "Customer"))
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            user.IsActive = !user.IsActive;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã {(user.IsActive ? "kích hoạt" : "khóa")} tài khoản khách hàng thành công.", status = user.IsActive });
        }

        private async Task RecalculateMembershipTier(User user)
        {
            var tiers = await _context.MembershipTiers.OrderBy(t => t.MinPoints).ToListAsync();
            MembershipTier? selectedTier = null;
            
            foreach (var tier in tiers)
            {
                if (user.LoyaltyPoints >= tier.MinPoints)
                {
                    selectedTier = tier;
                }
            }

            if (selectedTier != null)
            {
                user.MembershipTierId = selectedTier.TierId;
            }
        }
    }
}
