using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly AppDbContext _context;

        public TaskItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<TaskItem> GetAll()
        {
            throw new NotImplementedException();
        }

        public TaskItem? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(TaskItem entity)
        {
            throw new NotImplementedException();
        }

        public void Update(TaskItem entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
