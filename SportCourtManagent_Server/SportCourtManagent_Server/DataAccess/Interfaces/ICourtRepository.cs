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
        IEnumerable<Court> GetAll();
        Court? GetById(int id);
        void Add(Court entity);
        void Update(Court entity);
        void Delete(int id);

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
    }
}
