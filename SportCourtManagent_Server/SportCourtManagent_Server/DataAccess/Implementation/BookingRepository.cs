using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Include(b => b.Promotion)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Include(b => b.Promotion)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task AddAsync(Booking entity)
        {
            _context.Bookings.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking entity)
        {
            _context.Bookings.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Bookings.FindAsync(id);
            if (entity != null)
            {
                _context.Bookings.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasConflictingBookingAsync(int courtId, int slotId, DateTime bookingDate)
        {
            return await _context.Bookings.AnyAsync(b => b.CourtId == courtId 
                                           && b.SlotId == slotId 
                                           && b.BookingDate.Date == bookingDate.Date
                                           && b.Status != BookingStatus.Cancelled);
        }

        public async Task<BookingBillingResult> ProcessBookingBillingAsync  (CreateBookingRequestDto dto, decimal courtPrice)
        {
            decimal subTotal = courtPrice;
            var bookingServicesList = new List<BookingService>();
            decimal servicesTotal = 0;

            if (dto.BookingServices != null && dto.BookingServices.Any())
            {
                foreach (var svcDto in dto.BookingServices)
                {
                    var service = await _context.Services.FindAsync(svcDto.ServiceId);
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
                var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromoCode == dto.PromoCode);

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
            
            await _context.SaveChangesAsync();

            return new BookingBillingResult
            {
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                BookingServices = bookingServicesList,
                AppliedPromotion = appliedPromotion
            };
        }

        // --- Additional methods for Customer & Admin features ---

        /// <summary>Gets customer bookings asynchronous.</summary>
        public async Task<IEnumerable<Booking>> GetCustomerBookingsAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .Include(b => b.Promotion)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        /// <summary>Gets admin bookings with optional filters asynchronous.</summary>
        public async Task<IEnumerable<Booking>> GetAdminBookingsAsync(DateTime? date, int? courtTypeId, string? status)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .Include(b => b.Promotion)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(b => b.BookingDate.Date == date.Value.Date);
            }
            if (courtTypeId.HasValue)
            {
                query = query.Where(b => b.Court.CourtTypeId == courtTypeId.Value);
            }
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<Enums.BookingStatus>(status, true, out var st))
            {
                query = query.Where(b => b.Status == st);
            }

            return await query.OrderByDescending(b => b.BookingDate).ToListAsync();
        }

        /// <summary>Gets booking detail including related entities asynchronous.</summary>
        public async Task<Booking?> GetDetailAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .Include(b => b.Promotion)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }
    }
}
