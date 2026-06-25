using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetAll();
        Role? GetById(int id);
        void Add(Role entity);
        void Update(Role entity);
        void Delete(int id);
    }
}
