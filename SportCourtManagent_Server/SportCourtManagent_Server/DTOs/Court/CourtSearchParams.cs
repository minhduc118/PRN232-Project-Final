using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Court;

/// <summary>
/// Query parameters for court search endpoint (GET /api/courts).
/// Supports filtering by type, status, price range, availability,
/// full-text search, sorting, and pagination.
/// </summary>
public class CourtSearchParams
{
    /// <summary>Filter by sport type category.</summary>
    public int? CourtTypeId { get; set; }

    /// <summary>Filter by court availability status (Available, Booked, InUse, Maintenance).</summary>
    public string? Status { get; set; }

    /// <summary>Minimum price filter.</summary>
    public decimal? MinPrice { get; set; }

    /// <summary>Maximum price filter.</summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>Filter courts with availability on this date.</summary>
    public DateTime? Date { get; set; }

    /// <summary>Filter courts with a specific time slot available.</summary>
    public int? TimeSlotId { get; set; }

    /// <summary>Full-text search on court name.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Sort field: "price", "rating", "name" (default: "name").</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort direction: true = descending, false = ascending.</summary>
    public bool SortDescending { get; set; }

    /// <summary>Current page number (1-indexed, default: 1).</summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>Items per page (default: 10, max: 50).</summary>
    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
