using System;

namespace SportCourtManagent_Server.DTOs;

/// <summary>
/// Base pagination parameters for query requests.
/// </summary>
public class BaseFilterParams
{
  public int PageNumber { get; set; } = 1;
  public int PageSize { get; set; } = 10;
  public string? Keyword { get; set; }
}

/// <summary>
/// Filter parameters for promotions query.
/// </summary>
public class PromotionFilterParams : BaseFilterParams
{
  public bool? IsActive { get; set; }
}

/// <summary>
/// Filter parameters for bookings query.
/// </summary>
public class BookingFilterParams : BaseFilterParams
{
  public DateTime? FromDate { get; set; }
  public DateTime? ToDate { get; set; }
  public int? CourtTypeId { get; set; }
  public string? Status { get; set; }
}

/// <summary>
/// Filter parameters for tournaments query.
/// </summary>
public class TournamentFilterParams : BaseFilterParams
{
  public DateTime? FromDate { get; set; }
  public DateTime? ToDate { get; set; }
  public string? Status { get; set; }
}
