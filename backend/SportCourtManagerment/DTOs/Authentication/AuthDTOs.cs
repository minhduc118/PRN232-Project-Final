using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.DTOs.Authentication;

// ─────────────────────────────────────────────
//  Requests
// ─────────────────────────────────────────────

/// <summary>Payload for POST /api/auth/register.</summary>
public class RegisterRequest
{
  [Required(ErrorMessage = "Họ và tên không được để trống.")]
  [MaxLength(100)]
  public string FullName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Email không được để trống.")]
  [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
  [MaxLength(100)]
  public string Email { get; set; } = string.Empty;

  [MaxLength(15)]
  public string? Phone { get; set; }

  [Required(ErrorMessage = "Mật khẩu không được để trống.")]
  [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
  public string Password { get; set; } = string.Empty;

  [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
  [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
  public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>Payload for POST /api/auth/verify-email (OTP verification).</summary>
public class VerifyEmailRequest
{
  [Required(ErrorMessage = "Email không được để trống.")]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  [Required(ErrorMessage = "Mã OTP không được để trống.")]
  [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 chữ số.")]
  public string Otp { get; set; } = string.Empty;
}

/// <summary>Payload for POST /api/auth/login.</summary>
public class LoginRequest
{
  [Required(ErrorMessage = "Email không được để trống.")]
  [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
  public string Email { get; set; } = string.Empty;

  [Required(ErrorMessage = "Mật khẩu không được để trống.")]
  public string Password { get; set; } = string.Empty;
}

/// <summary>Payload for POST /api/auth/refresh-token.</summary>
public class RefreshTokenRequest
{
  [Required]
  public string AccessToken { get; set; } = string.Empty;

  [Required]
  public string RefreshToken { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────
//  Responses
// ─────────────────────────────────────────────

/// <summary>User data returned after successful login (no password).</summary>
public class UserDto
{
  public int UserId { get; set; }
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string? Phone { get; set; }
  public string? AvatarUrl { get; set; }

  /// <summary>Primary role name (Admin / Staff / Coach / Customer).</summary>
  public string Role { get; set; } = string.Empty;

  /// <summary>Membership tier name (Bronze / Silver / Gold / Platinum).</summary>
  public string? MembershipTier { get; set; }
}

/// <summary>Full auth response returned on successful login or token refresh.</summary>
public class AuthResponse
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public UserDto User { get; set; } = new();
}

/// <summary>Lightweight token-only response used by refresh-token endpoint.</summary>
public class TokenResponse
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
}
