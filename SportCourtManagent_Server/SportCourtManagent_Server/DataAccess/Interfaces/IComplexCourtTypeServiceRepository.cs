using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IComplexCourtTypeServiceRepository
    {
        Task<IEnumerable<ComplexCourtTypeService>> GetByComplexAndCourtTypeAsync(int complexId, int courtTypeId);
        Task<IEnumerable<ComplexCourtTypeService>> GetByComplexAsync(int complexId);
        Task<ComplexCourtTypeService?> GetByIdAsync(int offeringId);
        Task<bool> ExistsAsync(int complexId, int courtTypeId, int serviceId, int? excludeOfferingId = null);
        Task AddAsync(ComplexCourtTypeService entity);
        Task UpdateAsync(ComplexCourtTypeService entity);
        Task DeleteAsync(int offeringId);
    }
}
