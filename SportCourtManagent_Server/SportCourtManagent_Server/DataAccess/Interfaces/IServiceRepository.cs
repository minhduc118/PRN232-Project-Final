using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IServiceRepository
    {
        IEnumerable<Service> GetAll();
        Service? GetById(int id);
        void Add(Service entity);
        void Update(Service entity);
        void Delete(int id);
    }
}
