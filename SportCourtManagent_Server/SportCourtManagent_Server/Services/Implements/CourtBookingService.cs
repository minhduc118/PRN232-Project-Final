using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class CourtBookingService : ICourtBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInMemoryBookingRepository _inMemoryBookingRepository;

        public CourtBookingService(
            IBookingRepository bookingRepository,
            ICourtRepository courtRepository,
            ITimeSlotRepository timeSlotRepository,
            IUserRepository userRepository,
            IPromotionRepository promotionRepository,
            IServiceRepository serviceRepository,
            IPaymentRepository paymentRepository,
            IInMemoryBookingRepository inMemoryBookingRepository)
        {
            _bookingRepository = bookingRepository;
            _courtRepository = courtRepository;
            _timeSlotRepository = timeSlotRepository;
            _userRepository = userRepository;
            _promotionRepository = promotionRepository;
            _serviceRepository = serviceRepository;
            _paymentRepository = paymentRepository;
            _inMemoryBookingRepository = inMemoryBookingRepository;
        }

        public Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto dto, int userId)
        {
            var court = _courtRepository.GetById(dto.CourtId);
            if (court == null)
            {
                throw new ArgumentException($"Court with ID {dto.CourtId} does not exist.");
            }

            if (court.IsDeleted)
            {
                throw new ArgumentException("This court has been deleted and cannot be booked.");
            }

            if (court.Status == CourtStatus.Maintenance || court.Status == CourtStatus.Inactive)
            {
                throw new InvalidOperationException($"Court is currently under {court.Status} and cannot be booked.");
            }

            var timeSlot = _timeSlotRepository.GetById(dto.SlotId);
            if (timeSlot == null)
            {
                throw new ArgumentException($"Time slot with ID {dto.SlotId} does not exist.");
            }

            var isAlreadyBookedInDb = _bookingRepository.HasConflictingBooking(dto.CourtId, dto.SlotId, dto.BookingDate);

            var isAlreadyBookedInCache = _inMemoryBookingRepository.HasConflictingBooking(dto.CourtId, dto.SlotId, dto.BookingDate);

            if (isAlreadyBookedInDb || isAlreadyBookedInCache)
            {
                throw new InvalidOperationException("This court is already booked or reserved for the selected slot and date.");
            }

            decimal courtPrice = _courtRepository.GetCourtPrice(dto.CourtId, dto.SlotId, dto.BookingDate);

            var billingResult = _bookingRepository.ProcessBookingBilling(dto, courtPrice);

            string bookingCode = $"BK-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            var booking = new Booking
            {
                BookingCode = bookingCode,
                UserId = userId,
                CourtId = dto.CourtId,
                SlotId = dto.SlotId,
                BookingDate = dto.BookingDate.Date,
                StartTime = timeSlot.StartTime,
                EndTime = timeSlot.EndTime,
                SubTotal = billingResult.SubTotal,
                DiscountAmount = billingResult.DiscountAmount,
                TotalAmount = billingResult.TotalAmount,
                Status = BookingStatus.Pending,
                PromotionId = billingResult.AppliedPromotion?.PromotionId,
                BookingServices = billingResult.BookingServices
            };

            _inMemoryBookingRepository.Save(booking, TimeSpan.FromMinutes(5));

            return Task.FromResult(new BookingResponseDto
            {
                BookingId = 0,
                BookingCode = booking.BookingCode,
                UserId = booking.UserId,
                CourtId = booking.CourtId,
                CourtName = court.CourtName,
                SlotId = booking.SlotId,
                SlotName = timeSlot.SlotName,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                SubTotal = booking.SubTotal,
                DiscountAmount = booking.DiscountAmount,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status.ToString(),
                PromoCode = billingResult.AppliedPromotion?.PromoCode,
                BookingServices = booking.BookingServices.Select(bs => new BookingServiceResponseDto
                {
                    ServiceId = bs.ServiceId,
                    ServiceName = bs.Service?.ServiceName ?? "Unknown Service",
                    Quantity = bs.Quantity,
                    Price = bs.Service?.Price ?? 0,
                    TotalPrice = bs.TotalPrice
                }).ToList()
            });
        }



        public Task<BookingResponseDto> ConfirmPaymentAsync(ConfirmPaymentRequestDto dto)
        {
            var booking = _inMemoryBookingRepository.GetByCode(dto.BookingCode);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking session has expired or does not exist.");
            }

            _inMemoryBookingRepository.Remove(dto.BookingCode);

            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var pm))
            {
                throw new ArgumentException($"Invalid payment method '{dto.PaymentMethod}'. Valid methods are: VNPay, MoMo, BankTransfer, Cash, Wallet.");
            }

            booking.Status = BookingStatus.Confirmed;

            booking.User = null!;
            booking.Court = null!;
            booking.TimeSlot = null!;
            booking.Promotion = null!;
            foreach (var bs in booking.BookingServices)
            {
                bs.Service = null!;
            }

            _bookingRepository.Add(booking);

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = booking.TotalAmount,
                PaymentMethod = pm,
                TransactionId = dto.TransactionId,
                Status = PaymentStatus.Success,
                PaidAt = DateTime.UtcNow
            };
            _paymentRepository.Add(payment);

            var dbBooking = _bookingRepository.GetById(booking.BookingId);
            if (dbBooking == null)
            {
                throw new InvalidOperationException("Failed to retrieve booking from database after saving.");
            }

            return Task.FromResult(new BookingResponseDto
            {
                BookingId = dbBooking.BookingId,
                BookingCode = dbBooking.BookingCode,
                UserId = dbBooking.UserId,
                CourtId = dbBooking.CourtId,
                CourtName = dbBooking.Court?.CourtName ?? "Unknown Court",
                SlotId = dbBooking.SlotId,
                SlotName = dbBooking.TimeSlot?.SlotName ?? "Unknown Slot",
                BookingDate = dbBooking.BookingDate,
                StartTime = dbBooking.StartTime,
                EndTime = dbBooking.EndTime,
                SubTotal = dbBooking.SubTotal,
                DiscountAmount = dbBooking.DiscountAmount,
                TotalAmount = dbBooking.TotalAmount,
                Status = dbBooking.Status.ToString(),
                PromoCode = dbBooking.Promotion?.PromoCode,
                BookingServices = dbBooking.BookingServices.Select(bs => new BookingServiceResponseDto
                {
                    ServiceId = bs.ServiceId,
                    ServiceName = bs.Service?.ServiceName ?? "Unknown Service",
                    Quantity = bs.Quantity,
                    Price = bs.Service?.Price ?? 0,
                    TotalPrice = bs.TotalPrice
                }).ToList()
            });
        }

    }
}
