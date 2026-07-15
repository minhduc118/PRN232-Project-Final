using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SportCourtManagent_Server.DTOs.Court;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface ICourtComplexService
    {
        Task<PagedComplexResult> GetAllAsync(string? search, int? courtTypeId, int page, int pageSize);
        Task<CourtComplexDto?> GetByIdAsync(int id);
        Task<ComplexStatsDto> GetStatsAsync();
        Task<CourtComplexDto> CreateAsync(UpsertCourtComplexRequest request);
        Task<CourtComplexDto?> UpdateAsync(int id, UpsertCourtComplexRequest request);
        Task<bool> DeleteAsync(int id);
        Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string scheme, string host);
    }
}
