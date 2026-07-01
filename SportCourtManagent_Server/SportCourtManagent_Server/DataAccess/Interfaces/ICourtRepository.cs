using System;
using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtRepository
    {
        Task<IEnumerable<Court>> GetAllAsync();
        Task<Court?> GetByIdAsync(int id);
        Task AddAsync(Court entity);
        Task UpdateAsync(Court entity);
        Task DeleteAsync(int id);
        Task<decimal> GetCourtPriceAsync(int courtId, int slotId, DateTime date);
    }
}
