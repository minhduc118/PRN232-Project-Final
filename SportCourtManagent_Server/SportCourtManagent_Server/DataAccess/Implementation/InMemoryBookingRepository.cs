using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class InMemoryBookingRepository : IInMemoryBookingRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private static readonly ConcurrentDictionary<string, Booking> _pendingBookings = new();

        public InMemoryBookingRepository(IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory)
        {
            _memoryCache = memoryCache;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Save(Booking booking, TimeSpan expiration)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration)
                .RegisterPostEvictionCallback(OnBookingEvicted, _serviceScopeFactory);

            _memoryCache.Set(booking.BookingCode, booking, cacheEntryOptions);
            _pendingBookings[booking.BookingCode] = booking;
        }

        public Booking? GetByCode(string bookingCode)
        {
            _pendingBookings.TryGetValue(bookingCode, out var booking);
            return booking;
        }

        public IEnumerable<Booking> GetAllPending()
        {
            return _pendingBookings.Values;
        }

        public void Remove(string bookingCode)
        {
            _memoryCache.Remove(bookingCode);
            _pendingBookings.TryRemove(bookingCode, out _);
        }

        public bool HasConflictingBooking(int courtId, int slotId, DateTime bookingDate)
        {
            return _pendingBookings.Values.Any(b => b.CourtId == courtId 
                                                 && b.SlotId == slotId 
                                                 && b.BookingDate.Date == bookingDate.Date);
        }

        private static void OnBookingEvicted(object key, object? value, EvictionReason reason, object? state)
        {
            if (reason == EvictionReason.Expired && value is Booking booking && state is IServiceScopeFactory scopeFactory)
            {
                _pendingBookings.TryRemove(booking.BookingCode, out _);

                if (booking.BookingServices != null && booking.BookingServices.Any())
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var serviceRepo = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
                        foreach (var bs in booking.BookingServices)
                        {
                            var service = serviceRepo.GetById(bs.ServiceId);
                            if (service != null)
                            {
                                service.StockQty += bs.Quantity;
                                serviceRepo.Update(service);
                            }
                        }
                    }
                }
            }
        }
    }
}
