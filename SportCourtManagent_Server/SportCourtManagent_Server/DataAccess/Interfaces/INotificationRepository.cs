using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface INotificationRepository
    {
        IEnumerable<Notification> GetAll();
        Notification? GetById(int id);
        void Add(Notification entity);
        void Update(Notification entity);
        void Delete(int id);
    }
}
