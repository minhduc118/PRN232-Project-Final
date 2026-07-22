using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IPermissionMatrixRepository
    {
        Task<IReadOnlyList<PermissionMatrixEntry>> GetAllAsync();
        Task UpsertAllAsync(List<PermissionMatrixEntry> entries);
    }
}
