using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IWaitlistRepository
    {
        IEnumerable<Waitlist> GetAll();
        Waitlist? GetById(int id);
        void Add(Waitlist entity);
        void Update(Waitlist entity);
        void Delete(int id);
    }
}
