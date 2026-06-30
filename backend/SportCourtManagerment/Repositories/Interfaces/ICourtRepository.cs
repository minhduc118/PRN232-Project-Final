using SportCourtManagerment.Models;

namespace SportCourtManagerment.Repositories.Interfaces;

/// <summary>
/// Court-specific repository extending generic operations with
/// navigation-aware queries for listing, detail, and availability.
/// </summary>
public interface ICourtRepository : IGenericRepository<Court>
{
  /// <summary>
  /// Returns a queryable of courts with CourtType, CourtImages, and CourtPricings included.
  /// Suitable for OData endpoints and search/filter use cases.
  /// </summary>
  IQueryable<Court> GetCourtsQueryable();

  /// <summary>
  /// Returns full court detail with all navigations loaded:
  /// CourtType, CourtImages, CourtPricings→TimeSlot, Reviews→User.
  /// </summary>
  Task<Court?> GetCourtDetailAsync(int courtId);

  /// <summary>
  /// Returns all active time slots for a court on a given date,
  /// with pricing and booking status information.
  /// </summary>
  Task<Court?> GetCourtWithPricingsAsync(int courtId);
}
