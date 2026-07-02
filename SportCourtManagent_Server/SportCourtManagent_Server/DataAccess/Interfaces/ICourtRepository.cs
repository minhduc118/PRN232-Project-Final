using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    /// <summary>
    /// Court-specific repository extending basic operations with
    /// navigation-aware queries for listing, detail, and availability.
    /// </summary>
    public interface ICourtRepository
    {
        IQueryable<Court> GetCourtsQueryable();
        Task<Court?> GetCourtDetailAsync(int courtId);
        Task<Court?> GetCourtWithPricingsAsync(int courtId);

        Task<IEnumerable<Court>> GetAllAsync();
        Task<Court?> GetByIdAsync(int id);
        Task AddAsync(Court entity);
        Task UpdateAsync(Court entity);
        Task DeleteAsync(int id);
        Task<decimal> GetCourtPriceAsync(int courtId, int slotId, DateTime date);
    }
}
