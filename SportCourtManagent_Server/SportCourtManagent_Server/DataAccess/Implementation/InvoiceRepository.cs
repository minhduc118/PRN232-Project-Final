using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Invoice> GetAll()
        {
            throw new NotImplementedException();
        }

        public Invoice? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Invoice entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Invoice entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
