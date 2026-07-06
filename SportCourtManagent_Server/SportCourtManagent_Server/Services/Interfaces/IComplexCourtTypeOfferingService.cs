using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.ComplexService;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IComplexCourtTypeOfferingService
    {
        Task<IEnumerable<ComplexCourtTypeServiceDto>> GetByComplexAndCourtTypeAsync(int complexId, int courtTypeId);
        Task<IEnumerable<ComplexCourtTypeServiceDto>> GetByComplexAsync(int complexId);
        Task<ComplexCourtTypeServiceDto?> GetByIdAsync(int offeringId);
        Task<ComplexCourtTypeServiceDto> CreateAsync(int complexId, int courtTypeId, CreateComplexCourtTypeServiceRequest request);
        Task<ComplexCourtTypeServiceDto?> UpdateAsync(int offeringId, UpdateComplexCourtTypeServiceRequest request);
        Task<bool> DeleteAsync(int offeringId);
    }
}
