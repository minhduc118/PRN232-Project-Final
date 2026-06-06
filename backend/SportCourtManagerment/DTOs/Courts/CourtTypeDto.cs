namespace SportCourtManagerment.DTOs.Courts;

public class CourtTypeDto
{
  public int CourtTypeId { get; set; }
  public string TypeName { get; set; } = string.Empty;
  public bool IsActive { get; set; }
}
