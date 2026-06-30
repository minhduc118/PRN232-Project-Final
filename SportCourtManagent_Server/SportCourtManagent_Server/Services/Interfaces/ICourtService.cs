using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;

namespace SportCourtManagent_Server.Services.Interfaces
{
    /// <summary>
    /// Business logic for court search, detail, and availability.
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
    }
}
