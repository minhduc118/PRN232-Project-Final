using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;

namespace SportCourtManagent_Server.Services.Interfaces
{
    /// <summary>
    /// Business logic for court search, detail, availability, and CRUD management.
    /// </summary>
    public interface ICourtService
    {
        /// <summary>Searches courts with filters, sorting, and pagination.</summary>
        Task<PagedResult<CourtListDto>> SearchCourtsAsync(CourtSearchParams searchParams);

        /// <summary>Returns full court detail by ID, or null if not found.</summary>
        Task<CourtDetailDto?> GetCourtDetailAsync(int courtId);

        /// <summary>Returns time slot availability for a court on a specific date.</summary>
        Task<CourtAvailabilityDto?> GetCourtAvailabilityAsync(int courtId, DateTime date);

        /// <summary>Returns an IQueryable of courts for OData endpoint.</summary>
        IQueryable<CourtListDto> GetCourtsODataQueryable();

        Task<IEnumerable<CourtDto>> GetAllAsync(int? complexId, string? status);
        Task<CourtDto?> GetByIdAsync(int id);
        Task<CourtDto> CreateAsync(CourtDto dto);
        Task UpdateAsync(int id, CourtDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null);
    }
}
