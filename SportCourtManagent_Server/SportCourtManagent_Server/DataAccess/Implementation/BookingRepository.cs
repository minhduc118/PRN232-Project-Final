using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Booking> GetAll()
        {
            throw new NotImplementedException();
        }

        public Booking? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Booking entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Booking entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
