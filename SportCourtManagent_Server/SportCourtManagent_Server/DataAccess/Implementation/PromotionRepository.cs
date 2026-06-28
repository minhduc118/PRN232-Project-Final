using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
  public class PromotionRepository : IPromotionRepository
  {
    private readonly AppDbContext _context;

    public PromotionRepository(AppDbContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

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
