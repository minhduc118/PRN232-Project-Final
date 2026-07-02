namespace SportCourtManagerment.DTOs.Users;

public class UserSummaryDto
{
  public int UserId { get; set; }
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string? Phone { get; set; }
  public string? AvatarUrl { get; set; }
  public string Role { get; set; } = string.Empty;
  public bool IsActive { get; set; }
}
