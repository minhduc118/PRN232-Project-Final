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

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.Include(p => p.Booking).ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments.Include(p => p.Booking).FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        public async Task AddAsync(Payment entity)
        {
            _context.Payments.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment entity)
        {
            _context.Payments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Payments.FindAsync(id);
            if (entity != null)
            {
                _context.Payments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
