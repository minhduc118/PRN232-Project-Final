using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<AuditLog> GetAll()
        {
            throw new NotImplementedException();
        }

        public AuditLog? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(AuditLog entity)
        {
            throw new NotImplementedException();
        }

        public void Update(AuditLog entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
