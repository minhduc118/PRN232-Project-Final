using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IBookingRepository
    {
        IEnumerable<Booking> GetAll();
        Booking? GetById(int id);
        void Add(Booking entity);
        void Update(Booking entity);
        void Delete(int id);
        bool HasConflictingBooking(int courtId, int slotId, DateTime bookingDate);
        BookingBillingResult ProcessBookingBilling(CreateBookingRequestDto dto, decimal courtPrice);
    }
}
