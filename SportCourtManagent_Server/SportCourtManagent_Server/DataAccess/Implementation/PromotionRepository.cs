using System;
using System.Collections.Generic;
using System.Linq;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly AppDbContext _context;

        public PromotionRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Promotion> GetAll()
        {
            return _context.Promotions.ToList();
        }

        public Promotion? GetById(int id)
        {
            return _context.Promotions.Find(id);
        }

        public void Add(Promotion entity)
        {
            _context.Promotions.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Promotion entity)
        {
            _context.Promotions.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Promotions.Find(id);
            if (entity != null)
            {
                _context.Promotions.Remove(entity);
                _context.SaveChanges();
            }
        }

        public Promotion? GetByCode(string code)
        {
            try
            {
                var promotion = _context.Promotions.FirstOrDefault(p => p.PromoCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (promotion == null)
                {
                    return null;
                }
                return promotion;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException("An error occurred while retrieving the promotion by code.", ex);
            }
        }

        // --- Additional methods for Admin Promotion features ---

        /// <summary>Gets all promotions asynchronous.</summary>
        public async Task<IEnumerable<Promotion>> GetAllAsync()
        {
            return await _context.Promotions.ToListAsync();
        }

        /// <summary>Gets promotion by ID asynchronous.</summary>
        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        /// <summary>Gets promotion by promo code asynchronous.</summary>
        public async Task<Promotion?> GetByCodeAsync(string promoCode)
        {
            if (string.IsNullOrWhiteSpace(promoCode)) return null;
            return await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromoCode == promoCode.ToUpper());
        }

        /// <summary>Adds a new promotion asynchronous.</summary>
        public async Task AddAsync(Promotion entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _context.Promotions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>Updates an existing promotion asynchronous.</summary>
        public async Task UpdateAsync(Promotion entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Promotions.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>Deletes a promotion by ID asynchronous.</summary>
        public async Task DeleteAsync(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo != null)
            {
                _context.Promotions.Remove(promo);
                await _context.SaveChangesAsync();
            }
        }
    }
}
