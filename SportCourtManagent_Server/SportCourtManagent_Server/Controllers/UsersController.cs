using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Implements;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagement;
        private readonly IUserAccessService _userAccess;

        public UsersController(IUserManagementService userManagement, IUserAccessService userAccess)
        {
            _userManagement = userManagement ?? throw new ArgumentNullException(nameof(userManagement));
            _userAccess = userAccess ?? throw new ArgumentNullException(nameof(userAccess));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userManagement.GetPagedAsync(search, role, isActive, page, pageSize);
                return Ok(ApiResults.Ok(result, "Lấy danh sách người dùng thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        /// <summary>
        /// Trả về danh sách phẳng (không phân trang) các tài khoản Staff đang hoạt động,
        /// dùng cho dropdown "Quản lý phụ trách" trong form tổ hợp sân.
        /// </summary>
        [HttpGet("managers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetManagers()
        {
            try
            {
                var managers = await _userManagement.GetManagersAsync();
                return Ok(ApiResults.Ok(managers, "Lấy danh sách quản lý thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userManagement.GetByIdAsync(id);
                if (user == null)
                    return NotFound(ApiResults.Fail("Không tìm thấy người dùng.", 404));

                return Ok(ApiResults.Ok(user, "Lấy thông tin người dùng thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpPut("{id:int}/access")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAccess(int id, [FromBody] UpdateUserAccessRequest request)
        {
            try
            {
                var (user, error) = await _userAccess.UpdateAccessAsync(id, GetCurrentUserId(), request);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(UserMapper.ToSummaryDto(user!), "Cập nhật quyền người dùng thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpPut("{id:int}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request)
        {
            try
            {
                var (user, error) = await _userAccess.AssignRoleAsync(id, GetCurrentUserId(), request.Role);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(UserMapper.ToSummaryDto(user!), "Cập nhật vai trò thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] SetUserStatusRequest request)
        {
            try
            {
                var (user, error) = await _userAccess.SetStatusAsync(id, GetCurrentUserId(), request.IsActive);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(UserMapper.ToSummaryDto(user!),
                    request.IsActive ? "Đã kích hoạt tài khoản." : "Đã vô hiệu hóa tài khoản."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                if (!TryGetUserId(out int userId))
                    return Unauthorized(ApiResults.Fail("Không xác định được người dùng.", 401));

                var (data, error) = await _userManagement.UpdateProfileAsync(userId, request);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(data, "Cập nhật thông tin cá nhân thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!TryGetUserId(out int userId))
                    return Unauthorized(ApiResults.Fail("Không xác định được người dùng.", 401));

                var error = await _userManagement.ChangePasswordAsync(userId, request);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(null, "Thay đổi mật khẩu thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(claim) && int.TryParse(claim, out userId);
        }
    }
}
