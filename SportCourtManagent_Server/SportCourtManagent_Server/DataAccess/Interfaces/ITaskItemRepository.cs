using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ITaskItemRepository
    {
        IEnumerable<TaskItem> GetAll();
        TaskItem? GetById(int id);
        void Add(TaskItem entity);
        void Update(TaskItem entity);
        void Delete(int id);
    }
}
