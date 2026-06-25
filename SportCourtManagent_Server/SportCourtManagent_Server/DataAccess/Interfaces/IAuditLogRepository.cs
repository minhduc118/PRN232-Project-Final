using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IAuditLogRepository
    {
        IEnumerable<AuditLog> GetAll();
        AuditLog? GetById(int id);
        void Add(AuditLog entity);
        void Update(AuditLog entity);
        void Delete(int id);
    }
}
