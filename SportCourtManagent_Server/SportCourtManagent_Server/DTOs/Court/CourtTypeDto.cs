namespace SportCourtManagent_Server.DTOs.Court;

/// <summary>
/// DTO representing a sport court category.
/// </summary>
public class CourtTypeDto
{
    public int CourtTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>Number of active courts of this type.</summary>
    public int CourtCount { get; set; }
}
