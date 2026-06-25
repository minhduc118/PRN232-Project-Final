using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Payment> GetAll()
        {
            throw new NotImplementedException();
        }

        public Payment? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Payment entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Payment entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
