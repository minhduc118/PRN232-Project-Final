using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
  public interface IPromotionRepository
  {
    /// <summary>Gets all promotions asynchronous.</summary>
    Task<IEnumerable<Promotion>> GetAllAsync();

    /// <summary>Gets promotion by ID asynchronous.</summary>
    Task<Promotion?> GetByIdAsync(int id);

    /// <summary>Gets promotion by promo code asynchronous.</summary>
    Task<Promotion?> GetByCodeAsync(string promoCode);

    /// <summary>Adds a new promotion asynchronous.</summary>
    Task AddAsync(Promotion entity);

    /// <summary>Updates an existing promotion asynchronous.</summary>
    Task UpdateAsync(Promotion entity);

    /// <summary>Deletes a promotion by ID asynchronous.</summary>
    Task DeleteAsync(int id);
  }
}
