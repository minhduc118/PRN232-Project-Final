namespace SportCourtManagent_Server.DTOs.Court;

/// <summary>
/// Court availability response for a specific date.
/// Shows each time slot with price and booking status.
/// </summary>
public class CourtAvailabilityDto
{
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<AvailabilitySlotDto> Slots { get; set; } = new();
}

/// <summary>Individual time slot availability entry.</summary>
public class AvailabilitySlotDto
{
    public int SlotId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal Price { get; set; }

    /// <summary>Available | Held | Booked | Maintenance | Inactive</summary>
    public string Status { get; set; } = "Available";
}
