using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.Enums;
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
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Include(b => b.Promotion)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .ToList();
        }

        public Booking? GetById(int id)
        {
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Include(b => b.Promotion)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefault(b => b.BookingId == id);
        }

        public void Add(Booking entity)
        {
            _context.Bookings.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Booking entity)
        {
            _context.Bookings.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Bookings.Find(id);
            if (entity != null)
            {
                _context.Bookings.Remove(entity);
                _context.SaveChanges();
            }
        }

        public bool HasConflictingBooking(int courtId, int slotId, DateTime bookingDate)
        {
            return _context.Bookings.Any(b => b.CourtId == courtId 
                                           && b.SlotId == slotId 
                                           && b.BookingDate.Date == bookingDate.Date
                                           && b.Status != BookingStatus.Cancelled);
        }

        public BookingBillingResult ProcessBookingBilling(CreateBookingRequestDto dto, decimal courtPrice)
        {
            decimal subTotal = courtPrice;
            var bookingServicesList = new List<BookingService>();
            decimal servicesTotal = 0;

            if (dto.BookingServices != null && dto.BookingServices.Any())
            {
                foreach (var svcDto in dto.BookingServices)
                {
                    var service = _context.Services.Find(svcDto.ServiceId);
                    if (service == null)
                    {
                        throw new ArgumentException($"Service with ID {svcDto.ServiceId} does not exist.");
                    }

                    if (service.StockQty < svcDto.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for service '{service.ServiceName}'. Available: {service.StockQty}, Requested: {svcDto.Quantity}");
                    }

                    service.StockQty -= svcDto.Quantity;
                    _context.Services.Update(service);

                    var bookingService = new BookingService
                    {
                        ServiceId = service.ServiceId,
                        Quantity = svcDto.Quantity,
                        TotalPrice = service.Price * svcDto.Quantity,
                        Service = service
                    };

                    bookingServicesList.Add(bookingService);
                    servicesTotal += bookingService.TotalPrice;
                }
            }

            subTotal += servicesTotal;

            decimal discountAmount = 0;
            Promotion? appliedPromotion = null;

            if (!string.IsNullOrWhiteSpace(dto.PromoCode))
            {
                var promotion = _context.Promotions.FirstOrDefault(p => p.PromoCode == dto.PromoCode);

                if (promotion == null)
                {
                    throw new BadHttpRequestException($"Promotion code '{dto.PromoCode}' is invalid.");
                }

                if (dto.BookingDate.Date < promotion.StartDate.Date || dto.BookingDate.Date > promotion.EndDate.Date)
                {
                    throw new BadHttpRequestException("This promotion code is expired or not yet active.");
                }

                appliedPromotion = promotion;
                if (promotion.DiscountType == DiscountType.Percent)
                {
                    discountAmount = subTotal * (promotion.DiscountValue / 100);
                }
                else if (promotion.DiscountType == DiscountType.FixedAmount)
                {
                    discountAmount = promotion.DiscountValue;
                }

                if (discountAmount > subTotal)
                {
                    discountAmount = subTotal;
                }
            }

            decimal totalAmount = subTotal - discountAmount;
            
            _context.SaveChanges();

            return new BookingBillingResult
            {
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                BookingServices = bookingServicesList,
                AppliedPromotion = appliedPromotion
            };
        }
    }
}
