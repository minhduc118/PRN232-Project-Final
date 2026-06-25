using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IUserRoleRepository
    {
        IEnumerable<UserRole> GetAll();
        UserRole? GetById(int id);
        void Add(UserRole entity);
        void Update(UserRole entity);
        void Delete(int id);
    }
}
