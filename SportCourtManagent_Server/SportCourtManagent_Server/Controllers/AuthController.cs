using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Auth;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var error = await _authService.RegisterAsync(request);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(null, "Đăng ký tài khoản thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (data, error) = await _authService.LoginAsync(request);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(data, "Đăng nhập thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var (data, error) = await _authService.GoogleLoginAsync(request);
                if (error != null)
                    return BadRequest(ApiResults.Fail(error));

                return Ok(ApiResults.Ok(data, "Đăng nhập bằng Google thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                if (!TryGetUserId(out int userId))
                    return Unauthorized(ApiResults.Fail("Không xác định được người dùng.", 401));

                var user = await _authService.GetCurrentUserAsync(userId);
                if (user == null)
                    return NotFound(ApiResults.Fail("Người dùng không tồn tại.", 404));

                return Ok(ApiResults.Ok(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(claim) && int.TryParse(claim, out userId);
        }
    }
}
