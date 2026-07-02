using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtRepository : ICourtRepository
    {
        private readonly AppDbContext _context;

        public CourtRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Court> GetAll()
        {
            return _context.Courts.Where(c => !c.IsDeleted).ToList();
        }

        public Court? GetById(int id)
        {
            return _context.Courts.FirstOrDefault(c => c.CourtId == id && !c.IsDeleted);
        }

        public void Add(Court entity)
        {
            _context.Courts.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Court entity)
        {
            _context.Courts.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Courts.Find(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.SaveChanges();
            }
        }

        /// <inheritdoc/>
        public IQueryable<Court> GetCourtsQueryable()
        {
            return _context.Courts
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Complex)
                .Include(c => c.CourtType)
                .Include(c => c.CourtImages)
                .Include(c => c.CourtPricings)
                .Include(c => c.Reviews.Where(r => r.IsVisible));
        }

        /// <inheritdoc/>
        public async Task<Court?> GetCourtDetailAsync(int courtId)
        {
            return await _context.Courts
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Complex)
                .Include(c => c.CourtType)
                .Include(c => c.CourtImages)
                .Include(c => c.CourtPricings)
                    .ThenInclude(cp => cp.TimeSlot)
                .Include(c => c.Reviews.Where(r => r.IsVisible).OrderByDescending(r => r.ReviewId).Take(5))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.CourtId == courtId);
        }

        /// <inheritdoc/>
        public async Task<Court?> GetCourtWithPricingsAsync(int courtId)
        {
            return await _context.Courts
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.CourtPricings)
                    .ThenInclude(cp => cp.TimeSlot)
                .FirstOrDefaultAsync(c => c.CourtId == courtId);
        }
    }
}
