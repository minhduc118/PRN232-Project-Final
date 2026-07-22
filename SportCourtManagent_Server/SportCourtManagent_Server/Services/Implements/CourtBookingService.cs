using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;

        public CourtBookingService(
            IBookingRepository bookingRepository,
            ICourtRepository courtRepository,
            ITimeSlotRepository timeSlotRepository,
            IUserRepository userRepository,
            IPromotionRepository promotionRepository,
            IServiceRepository serviceRepository,
            IPaymentRepository paymentRepository,
            IInMemoryBookingRepository inMemoryBookingRepository,
            AppDbContext context)
        {
            _bookingRepository = bookingRepository;
            _courtRepository = courtRepository;
            _timeSlotRepository = timeSlotRepository;
            _userRepository = userRepository;
            _promotionRepository = promotionRepository;
            _serviceRepository = serviceRepository;
            _paymentRepository = paymentRepository;
            _inMemoryBookingRepository = inMemoryBookingRepository;
            _context = context;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto dto, int userId)
        {
            var court = await _courtRepository.GetByIdAsync(dto.CourtId);
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

            var timeSlot = await _timeSlotRepository.GetByIdAsync(dto.SlotId);
            if (timeSlot == null)
            {
                throw new ArgumentException($"Time slot with ID {dto.SlotId} does not exist.");
            }

            var isAlreadyBookedInDb = await _bookingRepository.HasConflictingBookingAsync(dto.CourtId, dto.SlotId, dto.BookingDate);

            if (isAlreadyBookedInDb)
            {
                throw new InvalidOperationException("Sân đấu này đã được đặt trong khung giờ đã chọn.");
            }

            decimal courtPrice = await _courtRepository.GetCourtPriceAsync(dto.CourtId, dto.SlotId, dto.BookingDate);

            var billingResult = await _bookingRepository.ProcessBookingBillingAsync(dto, courtPrice);

            // Kiểm tra ví
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 10000000m };
                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            if (wallet.Balance < billingResult.TotalAmount)
            {
                throw new InvalidOperationException($"Số dư ví không đủ. Chi phí đặt sân là {billingResult.TotalAmount:N0}đ nhưng ví của bạn chỉ còn {wallet.Balance:N0}đ. Vui lòng nạp thêm tiền.");
            }

            // Trừ tiền ví
            wallet.Balance -= billingResult.TotalAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

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
                Status = BookingStatus.Confirmed, // Confirmed immediately
                PromotionId = billingResult.AppliedPromotion?.PromotionId,
                BookingServices = billingResult.BookingServices
            };

            await _bookingRepository.AddAsync(booking);

            // Ghi nhận Payment
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = booking.TotalAmount,
                PaymentMethod = PaymentMethod.Wallet,
                TransactionId = $"WT-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                Status = PaymentStatus.Success,
                PaidAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment);

            // Ghi nhận WalletTransaction
            var wt = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = -booking.TotalAmount,
                Type = WalletTransactionType.Payment,
                BookingId = booking.BookingId,
                Description = $"Thanh toán đặt sân {booking.BookingCode}",
                CreatedAt = DateTime.UtcNow
            };
            await _context.WalletTransactions.AddAsync(wt);
            await _context.SaveChangesAsync();

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
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
            };
        }



        public async Task<BookingResponseDto> ConfirmPaymentAsync(ConfirmPaymentRequestDto dto)
        {
            var booking = await _inMemoryBookingRepository.GetByCodeAsync(dto.BookingCode);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking session has expired or does not exist.");
            }

            await _inMemoryBookingRepository.RemoveAsync(dto.BookingCode);

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

            await _bookingRepository.AddAsync(booking);

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = booking.TotalAmount,
                PaymentMethod = pm,
                TransactionId = dto.TransactionId,
                Status = PaymentStatus.Success,
                PaidAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment);

            var dbBooking = await _bookingRepository.GetByIdAsync(booking.BookingId);
            if (dbBooking == null)
            {
                throw new InvalidOperationException("Failed to retrieve booking from database after saving.");
            }

            return new BookingResponseDto
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
            };
        }

    }
}
