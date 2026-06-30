using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    /// <summary>
    /// Review-specific repository with court-scoped queries and rating aggregation.
    /// </summary>
    public interface IReviewRepository
    {
        IEnumerable<Review> GetAll();
        Review? GetById(int id);
        void Add(Review entity);
        void Update(Review entity);
        void Delete(int id);

        /// <summary>
        /// Returns a queryable of visible reviews for a specific court,
        /// including the reviewing User navigation.
        /// </summary>
        IQueryable<Review> GetReviewsByCourtQueryable(int courtId);

        /// <summary>
        /// Returns aggregated rating statistics for a court:
        /// average rating, total count, and per-star distribution.
        /// </summary>
        Task<(double avgRating, int totalCount, Dictionary<int, int> distribution)>
            GetCourtRatingSummaryAsync(int courtId);

        /// <summary>Returns an un-materialized queryable filtered by a predicate.</summary>
        IQueryable<Review> FindBy(Expression<Func<Review, bool>> predicate);

        Task AddAsync(Review entity);

        Task<int> SaveChangesAsync();
    }
}
