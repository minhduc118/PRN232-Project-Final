using SportCourtManagent_Server.DTOs.Review;

namespace SportCourtManagent_Server.DTOs.Court;

/// <summary>
/// Full court detail DTO returned by GET /api/courts/{id}.
/// Includes all nested navigation data for the court detail page.
/// </summary>
public class CourtDetailDto
{
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public string CourtCode { get; set; } = string.Empty;
    public string? CourtSize { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public decimal PricePerHour { get; set; }
    public string Status { get; set; } = string.Empty;

    // Nested DTOs
    public CourtTypeDto CourtType { get; set; } = null!;
    public List<CourtImageDto> Images { get; set; } = new();
    public List<CourtPricingDto> Pricings { get; set; } = new();
    public CourtReviewSummaryDto ReviewSummary { get; set; } = null!;
}

/// <summary>Court image gallery item.</summary>
public class CourtImageDto
{
    public int CourtImageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

/// <summary>Court pricing configuration per time slot.</summary>
public class CourtPricingDto
{
    public int PricingId { get; set; }
    public int SlotId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string DayType { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
