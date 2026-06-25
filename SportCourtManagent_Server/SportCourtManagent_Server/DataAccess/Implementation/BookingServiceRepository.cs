using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class BookingServiceRepository : IBookingServiceRepository
    {
        private readonly AppDbContext _context;

        public BookingServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<BookingService> GetAll()
        {
            throw new NotImplementedException();
        }

        public BookingService? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(BookingService entity)
        {
            throw new NotImplementedException();
        }

        public void Update(BookingService entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
