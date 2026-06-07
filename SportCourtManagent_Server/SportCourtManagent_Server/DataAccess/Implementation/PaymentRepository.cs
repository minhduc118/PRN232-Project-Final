using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            return _context.Payments.Include(p => p.Booking).ToList();
        }

        public Payment? GetById(int id)
        {
            return _context.Payments.Include(p => p.Booking).FirstOrDefault(p => p.PaymentId == id);
        }

        public void Add(Payment entity)
        {
            _context.Payments.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Payment entity)
        {
            _context.Payments.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Payments.Find(id);
            if (entity != null)
            {
                _context.Payments.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
