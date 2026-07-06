using SportCourtManagerment.DTOs.Home;

namespace SportCourtManagerment.Services.Interfaces;

/// <summary>
/// Business logic for the customer landing page aggregate data.
/// </summary>
public interface IHomeService
{
  /// <summary>
  /// Returns all data needed for the customer home page in a single call:
  /// court types, top-rated courts, active promotions, and statistics.
  /// </summary>
  Task<HomeDataDto> GetHomeDataAsync();
}
