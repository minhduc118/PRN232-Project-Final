namespace SportCourtManagent_Server.DTOs.Review;

/// <summary>
/// DTO for a single review entry displayed on the court detail page.
/// </summary>
public class ReviewDto
{
    public int ReviewId { get; set; }
    public int BookingId { get; set; }
    public int CourtId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Aggregated review statistics for a court.
/// Used in court detail page header and search result cards.
/// </summary>
public class CourtReviewSummaryDto
{
    /// <summary>Average rating rounded to 1 decimal (0 if no reviews).</summary>
    public double AverageRating { get; set; }

    /// <summary>Total number of visible reviews.</summary>
    public int TotalReviews { get; set; }

    /// <summary>Count of reviews per star level: {5: 20, 4: 15, 3: 8, 2: 3, 1: 1}.</summary>
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}
