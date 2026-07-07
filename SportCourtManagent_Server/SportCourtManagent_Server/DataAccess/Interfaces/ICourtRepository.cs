using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    /// <summary>
    /// Court-specific repository extending basic operations with
    /// navigation-aware queries for listing, detail, and availability.
    /// </summary>
    public interface ICourtRepository
    {
        IEnumerable<Court> GetAll();
        Court? GetById(int id);
        void Add(Court entity);
        void Update(Court entity);
        void Delete(int id);

        Task<IEnumerable<Court>> GetAllAsync();
        Task<Court?> GetByIdAsync(int id);
        Task AddAsync(Court entity);
        Task UpdateAsync(Court entity);
        Task DeleteAsync(int id);

        /// <summary>
        /// Returns a queryable of courts with CourtType, CourtImages, and CourtPricings included.
        /// Suitable for OData endpoints and search/filter use cases.
        /// </summary>
        IQueryable<Court> GetCourtsQueryable();

        /// <summary>
        /// Returns full court detail with all navigations loaded:
        /// CourtType, CourtImages, CourtPricings→TimeSlot, Reviews→User.
        /// </summary>
        Task<Court?> GetCourtDetailAsync(int courtId);

        /// <summary>
        /// Returns court with pricing and timeslot info for availability checking.
        /// </summary>
        Task<Court?> GetCourtWithPricingsAsync(int courtId);
        Task<decimal> GetCourtPriceAsync(int courtId, int slotId, DateTime date);
        Task<IEnumerable<Court>> GetCourtsByComplexAsync(int complexId);

        Task<IEnumerable<Court>> GetAllWithDetailsAsync(int? complexId = null, string? status = null);
        Task<Court?> GetByIdWithDetailsAsync(int id);
        Task SoftDeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null);
    }
}
