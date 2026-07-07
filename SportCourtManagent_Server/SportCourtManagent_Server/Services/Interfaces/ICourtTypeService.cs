using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Court;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface ICourtTypeService
    {
        Task<IEnumerable<CourtTypeDto>> GetAllActiveAsync();
    }
}
