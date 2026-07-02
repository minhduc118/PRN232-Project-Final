using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    /// <summary>
    /// Review repository with court-scoped queries and aggregation logic.
    /// </summary>
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Review> GetAll()
        {
            return _context.Reviews.ToList();
        }

        public Review? GetById(int id)
        {
            return _context.Reviews.Find(id);
        }

        public void Add(Review entity)
        {
            _context.Reviews.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Review entity)
        {
            _context.Reviews.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Reviews.Find(id);
            if (entity != null)
            {
                _context.Reviews.Remove(entity);
                _context.SaveChanges();
            }
        }

        /// <inheritdoc/>
        public IQueryable<Review> GetReviewsByCourtQueryable(int courtId)
        {
            return _context.Reviews
                .AsNoTracking()
                .Where(r => r.CourtId == courtId && r.IsVisible)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReviewId);
        }

        /// <inheritdoc/>
        public async Task<(double avgRating, int totalCount, Dictionary<int, int> distribution)>
            GetCourtRatingSummaryAsync(int courtId)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.CourtId == courtId && r.IsVisible)
                .Select(r => (int)r.Rating)
                .ToListAsync();

            if (reviews.Count == 0)
                return (0, 0, new Dictionary<int, int>
                {
                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                });

            var avgRating = reviews.Average();
            var distribution = Enumerable.Range(1, 5)
                .ToDictionary(star => star, star => reviews.Count(r => r == star));

            return (Math.Round(avgRating, 1), reviews.Count, distribution);
        }

        /// <inheritdoc/>
        public IQueryable<Review> FindBy(Expression<Func<Review, bool>> predicate)
        {
            return _context.Reviews.AsNoTracking().Where(predicate);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Review entity)
        {
            await _context.Reviews.AddAsync(entity);
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
