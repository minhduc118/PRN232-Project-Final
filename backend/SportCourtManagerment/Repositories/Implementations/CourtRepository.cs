using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.Models;
using SportCourtManagerment.Repositories.Interfaces;

namespace SportCourtManagerment.Repositories.Implementations;

/// <summary>
/// Court repository with eager-loading queries for listing, detail, and pricing.
/// </summary>
public class CourtRepository : GenericRepository<Court>, ICourtRepository
{
  public CourtRepository(ApplicationDbContext context) : base(context) { }

  /// <inheritdoc/>
  public IQueryable<Court> GetCourtsQueryable()
  {
    return _context.Courts
      .AsNoTracking()
      .Include(c => c.CourtType)
      .Include(c => c.CourtImages.OrderBy(ci => ci.SortOrder))
      .Include(c => c.CourtPricings)
      .Include(c => c.Reviews.Where(r => r.IsVisible));
  }

  /// <inheritdoc/>
  public async Task<Court?> GetCourtDetailAsync(int courtId)
  {
    return await _context.Courts
      .AsNoTracking()
      .Include(c => c.CourtType)
      .Include(c => c.CourtImages.OrderBy(ci => ci.SortOrder))
      .Include(c => c.CourtPricings)
        .ThenInclude(cp => cp.TimeSlot)
      .Include(c => c.Reviews.Where(r => r.IsVisible).OrderByDescending(r => r.CreatedAt).Take(5))
        .ThenInclude(r => r.User)
      .FirstOrDefaultAsync(c => c.CourtId == courtId);
  }

  /// <inheritdoc/>
  public async Task<Court?> GetCourtWithPricingsAsync(int courtId)
  {
    return await _context.Courts
      .AsNoTracking()
      .Include(c => c.CourtPricings)
        .ThenInclude(cp => cp.TimeSlot)
      .FirstOrDefaultAsync(c => c.CourtId == courtId);
  }
}
