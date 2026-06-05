using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Authentication;
using SportCourtManagerment.Models;
using SportCourtManagerment.Services;
using SportCourtManagerment.Services.Email;

namespace SportCourtManagerment.Controllers.Authentication;

/// <summary>
/// Handles all authentication and authorization endpoints:
/// Register → VerifyEmail (OTP) → Login → RefreshToken → Logout → Me
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
  private readonly ApplicationDbContext _db;
  private readonly TokenService         _tokenService;
  private readonly IEmailService        _emailService;
  private readonly IConfiguration       _config;

  public AuthController(
    ApplicationDbContext db,
    TokenService         tokenService,
    IEmailService        emailService,
    IConfiguration       config)
  {
    _db           = db;
    _tokenService = tokenService;
    _emailService = emailService;
    _config       = config;
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  POST /api/auth/register
  // ──────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Registers a new customer account.
  /// Sets IsEmailVerified = false and sends a 6-digit OTP to the user's email.
  /// </summary>
  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
  {
    // 1. Check for duplicate email
    if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
    {
      return Conflict(ApiResponse<object>.Fail(
        "Email này đã được sử dụng. Vui lòng chọn email khác.", 409));
    }

    // 2. Generate 6-digit OTP
    var otp    = new Random().Next(100000, 999999).ToString();
    var expiry = DateTime.UtcNow.AddMinutes(10);

    // 3. Build new user (account not yet active — waiting for OTP)
    var customerRole = await _db.Roles.FirstAsync(r => r.RoleName == "Customer");
    var bronzeTier   = await _db.MembershipTiers.FirstAsync(t => t.TierName == "Bronze");

    var user = new User
    {
      FullName              = dto.FullName,
      Email                 = dto.Email,
      Phone                 = dto.Phone,
      PasswordHash          = BCrypt.Net.BCrypt.HashPassword(dto.Password),
      IsActive              = false,          // activated after OTP verification
      IsEmailVerified       = false,
      VerificationToken     = otp,
      VerificationTokenExpiry = expiry,
      MembershipTierId      = bronzeTier.TierId,
      CreatedAt             = DateTime.UtcNow,
    };

    _db.Users.Add(user);
    await _db.SaveChangesAsync();

    // 4. Assign Customer role
    _db.UserRoles.Add(new UserRole
    {
      UserId     = user.UserId,
      RoleId     = customerRole.RoleId,
      AssignedAt = DateTime.UtcNow,
    });
    await _db.SaveChangesAsync();

    // 5. Send OTP via email (or console log in dev)
    await _emailService.SendOtpEmailAsync(user.Email, user.FullName, otp);

    return Ok(ApiResponse<object>.Ok(
      new { email = user.Email },
      "Đăng ký thành công. Mã OTP xác thực đã được gửi tới email của bạn."));
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  POST /api/auth/verify-email
  // ──────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Verifies the 6-digit OTP sent to the user's email during registration.
  /// On success: IsEmailVerified = true, IsActive = true, OTP is cleared.
  /// </summary>
  [HttpPost("verify-email")]
  public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest dto)
  {
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (user is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tài khoản với email này.", 404));

    if (user.IsEmailVerified)
      return BadRequest(ApiResponse<object>.Fail("Tài khoản này đã được xác thực trước đó."));

    // Compare OTP
    if (user.VerificationToken != dto.Otp)
      return BadRequest(ApiResponse<object>.Fail("Mã OTP không chính xác."));

    // Check expiry
    if (user.VerificationTokenExpiry < DateTime.UtcNow)
      return BadRequest(ApiResponse<object>.Fail(
        "Mã OTP đã hết hạn. Vui lòng đăng ký lại hoặc yêu cầu mã mới."));

    // Activate account
    user.IsEmailVerified        = true;
    user.IsActive               = true;
    user.VerificationToken      = null;
    user.VerificationTokenExpiry = null;
    user.UpdatedAt              = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    return Ok(ApiResponse<object>.Ok(null,
      "Xác thực tài khoản thành công. Bạn đã có thể đăng nhập."));
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  POST /api/auth/login
  // ──────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Authenticates the user with email + password.
  /// Returns a signed JWT Access Token and an opaque Refresh Token.
  /// </summary>
  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest dto)
  {
    // Load user with roles and membership — needed for token claims and DTO
    var user = await _db.Users
      .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
      .Include(u => u.MembershipTier)
      .FirstOrDefaultAsync(u => u.Email == dto.Email);

    // Generic error message to avoid email enumeration attacks
    const string invalidMsg = "Email hoặc mật khẩu không chính xác.";

    if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
      return Unauthorized(ApiResponse<object>.Fail(invalidMsg, 401));

    if (!user.IsEmailVerified)
      return StatusCode(403, ApiResponse<object>.Fail(
        "Tài khoản chưa được xác thực email. Vui lòng kiểm tra hộp thư và nhập mã OTP.", 403));

    if (!user.IsActive)
      return StatusCode(403, ApiResponse<object>.Fail(
        "Tài khoản đã bị khoá. Vui lòng liên hệ quản trị viên.", 403));

    // Generate tokens
    var accessToken  = _tokenService.GenerateAccessToken(user);
    var refreshToken = _tokenService.GenerateRefreshToken();

    // Persist refresh token
    user.RefreshToken       = refreshToken;
    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
      int.Parse(_config["Jwt:RefreshTokenExpirationInDays"]!));
    await _db.SaveChangesAsync();

    var response = new AuthResponse
    {
      AccessToken  = accessToken,
      RefreshToken = refreshToken,
      User         = MapToDto(user),
    };

    return Ok(ApiResponse<AuthResponse>.Ok(response, "Đăng nhập thành công."));
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  POST /api/auth/refresh-token
  // ──────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Issues a new Access Token + Refresh Token pair.
  ///
  /// How it works:
  ///   1. Read claims from the EXPIRED Access Token (signature is still checked,
  ///      but the expiry check is intentionally skipped — see TokenService).
  ///   2. Look up the user in the database using the UserId claim.
  ///   3. Compare the provided RefreshToken with the one stored in the database
  ///      (plain string equality — the stored value acts as a single-use secret).
  ///   4. Check RefreshToken expiry date.
  ///   5. Issue new Access + Refresh tokens and persist the new refresh token.
  /// </summary>
  [HttpPost("refresh-token")]
  public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest dto)
  {
    // Step 1: Extract claims from expired access token (skip lifetime check)
    var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
    if (principal is null)
      return BadRequest(ApiResponse<object>.Fail("Access token không hợp lệ."));

    // Step 2: Get UserId from claims
    var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdClaim, out var userId))
      return BadRequest(ApiResponse<object>.Fail("Không thể đọc thông tin từ token."));

    // Step 3: Load user with roles
    var user = await _db.Users
      .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
      .Include(u => u.MembershipTier)
      .FirstOrDefaultAsync(u => u.UserId == userId);

    if (user is null)
      return Unauthorized(ApiResponse<object>.Fail("Tài khoản không tồn tại.", 401));

    // Step 4: Compare refresh token (plain string equality — stored in DB)
    //         and check expiry
    if (user.RefreshToken != dto.RefreshToken ||
        user.RefreshTokenExpiry <= DateTime.UtcNow)
    {
      return Unauthorized(ApiResponse<object>.Fail(
        "Refresh token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.", 401));
    }

    // Step 5: Issue brand-new token pair (token rotation)
    var newAccessToken  = _tokenService.GenerateAccessToken(user);
    var newRefreshToken = _tokenService.GenerateRefreshToken();

    user.RefreshToken       = newRefreshToken;
    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
      int.Parse(_config["Jwt:RefreshTokenExpirationInDays"]!));
    await _db.SaveChangesAsync();

    var response = new TokenResponse
    {
      AccessToken  = newAccessToken,
      RefreshToken = newRefreshToken,
    };

    return Ok(ApiResponse<TokenResponse>.Ok(response, "Làm mới token thành công."));
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  POST /api/auth/logout
  // ──────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Clears the Refresh Token stored in the database for the current user,
  /// effectively invalidating all future token refresh attempts.
  /// The Access Token itself will expire naturally after its short TTL.
  /// </summary>
  [Authorize]
  [HttpPost("logout")]
  public async Task<IActionResult> Logout()
  {
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdClaim, out var userId))
      return Unauthorized(ApiResponse<object>.Fail("Không thể xác định người dùng.", 401));

    var user = await _db.Users.FindAsync(userId);
    if (user is not null)
    {
      user.RefreshToken       = null;
      user.RefreshTokenExpiry = null;
      await _db.SaveChangesAsync();
    }

    return Ok(ApiResponse<object>.Ok(null, "Đăng xuất thành công."));
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  GET /api/auth/me
  // ──────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Returns the profile of the currently authenticated user.
  /// Used by the frontend to re-hydrate auth state after a page refresh.
  /// </summary>
  [Authorize]
  [HttpGet("me")]
  public async Task<IActionResult> Me()
  {
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdClaim, out var userId))
      return Unauthorized(ApiResponse<object>.Fail("Không thể xác định người dùng.", 401));

    var user = await _db.Users
      .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
      .Include(u => u.MembershipTier)
      .FirstOrDefaultAsync(u => u.UserId == userId);

    if (user is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tài khoản.", 404));

    return Ok(ApiResponse<UserDto>.Ok(MapToDto(user),
      "Lấy thông tin tài khoản thành công."));
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  Private helpers
  // ──────────────────────────────────────────────────────────────────────────

  private static UserDto MapToDto(User user) => new()
  {
    UserId        = user.UserId,
    FullName      = user.FullName,
    Email         = user.Email,
    Phone         = user.Phone,
    AvatarUrl     = user.AvatarUrl,
    Role          = user.UserRoles.FirstOrDefault()?.Role.RoleName ?? "Customer",
    MembershipTier = user.MembershipTier?.TierName,
  };
}
