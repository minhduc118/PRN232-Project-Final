using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Services;

/// <summary>Generates and validates JWT Access Tokens and Refresh Tokens.</summary>
public class TokenService
{
  private readonly IConfiguration _config;

  public TokenService(IConfiguration config) => _config = config;

  // ─────────────────────────────────────────────────────────────────────────
  //  Access Token (JWT)
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Generates a signed JWT Access Token that encodes the user's
  /// identity (UserId, Email, FullName) and all their assigned roles as claims.
  /// The token is signed using HMAC-SHA256 and expires based on the
  /// "Jwt:AccessTokenExpirationInMinutes" config value (default 15 min).
  /// </summary>
  public string GenerateAccessToken(User user)
  {
    var jwtSection       = _config.GetSection("Jwt");
    var secret           = jwtSection["Secret"]!;
    var issuer           = jwtSection["Issuer"]!;
    var audience         = jwtSection["Audience"]!;
    var expirationMins   = int.Parse(jwtSection["AccessTokenExpirationInMinutes"]!);

    var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Build claims — these are embedded in the JWT payload
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
      new(ClaimTypes.Email,          user.Email),
      new(ClaimTypes.Name,           user.FullName),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // unique token id
    };

    // Add one Role claim per role assigned to the user
    foreach (var userRole in user.UserRoles)
    {
      claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
    }

    var token = new JwtSecurityToken(
      issuer:             issuer,
      audience:           audience,
      claims:             claims,
      expires:            DateTime.UtcNow.AddMinutes(expirationMins),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  // ─────────────────────────────────────────────────────────────────────────
  //  Refresh Token (opaque random string)
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Generates a cryptographically random Refresh Token string.
  /// This value is stored in the User row in the database and compared
  /// directly (string equality) when the client requests a token refresh.
  /// </summary>
  public string GenerateRefreshToken() => Guid.NewGuid().ToString("N");

  // ─────────────────────────────────────────────────────────────────────────
  //  Read expired Access Token (for refresh-token flow)
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Extracts the ClaimsPrincipal from an Access Token that MAY already be expired.
  /// Used exclusively in the refresh-token flow: we need to read the UserId / Email
  /// from the old token WITHOUT rejecting it because it is past its expiry time.
  ///
  /// Validation rules applied here:
  ///   ✅ Validate Issuer, Audience, and Signature  (must still be valid)
  ///   ❌ ValidateLifetime = false  (intentionally skip expiry check)
  /// </summary>
  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken)
  {
    var secret = _config["Jwt:Secret"]!;
    var validationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
      ValidateIssuer           = true,
      ValidIssuer              = _config["Jwt:Issuer"],
      ValidateAudience         = true,
      ValidAudience            = _config["Jwt:Audience"],
      ValidateLifetime         = false, // ← key: skip expiry check intentionally
    };

    try
    {
      var principal = new JwtSecurityTokenHandler()
        .ValidateToken(accessToken, validationParameters, out var securityToken);

      // Ensure the token was signed with HMAC-SHA256 (not "none" algorithm attack)
      if (securityToken is not JwtSecurityToken jwt ||
          !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
            StringComparison.OrdinalIgnoreCase))
        return null;

      return principal;
    }
    catch
    {
      return null;
    }
  }
}
