using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            return _context.BookingServices
                .Include(bs => bs.Booking)
                .Include(bs => bs.Service)
                .ToList();
        }

        public BookingService? GetById(int id)
        {
            return _context.BookingServices
                .Include(bs => bs.Booking)
                .Include(bs => bs.Service)
                .FirstOrDefault(bs => bs.BookingServiceId == id);
        }

        public void Add(BookingService entity)
        {
            _context.BookingServices.Add(entity);
            _context.SaveChanges();
        }

        public void Update(BookingService entity)
        {
            _context.BookingServices.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.BookingServices.Find(id);
            if (entity != null)
            {
                _context.BookingServices.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
