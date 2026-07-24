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

            var targetSlotIds = (dto.SlotIds != null && dto.SlotIds.Any()) 
                ? dto.SlotIds.Distinct().OrderBy(s => s).ToList() 
                : new List<int> { dto.SlotId };

            var timeSlots = await _context.TimeSlots
                .Where(s => targetSlotIds.Contains(s.SlotId))
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            if (timeSlots.Count == 0)
            {
                throw new ArgumentException("Khung giờ đã chọn không tồn tại.");
            }

            // Validate: reject booking for past date/time
            var nowLocal = DateTime.Now;
            if (dto.BookingDate.Date < nowLocal.Date)
            {
                throw new ArgumentException("Ngày đặt sân đã qua, không thể đặt sân.");
            }
            if (dto.BookingDate.Date == nowLocal.Date && timeSlots.Min(s => s.StartTime) <= nowLocal.TimeOfDay)
            {
                throw new ArgumentException("Khung giờ đặt sân đã qua thời gian hiện tại, không thể đặt sân.");
            }

            foreach (var sId in targetSlotIds)
            {
                var isAlreadyBookedInDb = await _bookingRepository.HasConflictingBookingAsync(dto.CourtId, sId, dto.BookingDate);
                if (isAlreadyBookedInDb)
                {
                    throw new InvalidOperationException($"Sân đấu này đã được đặt trong khung giờ đã chọn.");
                }
            }

            decimal totalCourtPrice = 0;
            foreach (var sId in targetSlotIds)
            {
                totalCourtPrice += await _courtRepository.GetCourtPriceAsync(dto.CourtId, sId, dto.BookingDate);
            }

            var billingResult = await _bookingRepository.ProcessBookingBillingAsync(dto, totalCourtPrice);

            string bookingCode = $"BK-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
            var primarySlot = timeSlots.First();
            var startTime = timeSlots.Min(s => s.StartTime);
            var endTime = timeSlots.Max(s => s.EndTime);

            var booking = new Booking
            {
                BookingCode = bookingCode,
                UserId = userId,
                CourtId = dto.CourtId,
                SlotId = primarySlot.SlotId,
                BookingDate = dto.BookingDate.Date,
                StartTime = startTime,
                EndTime = endTime,
                SubTotal = billingResult.SubTotal,
                DiscountAmount = billingResult.DiscountAmount,
                TotalAmount = billingResult.TotalAmount,
                Status = BookingStatus.Confirmed,
                ExpiredAt = null,
                PromotionId = billingResult.AppliedPromotion?.PromotionId,
                BookingServices = billingResult.BookingServices
            };

            // Check & deduct wallet balance
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 10000000m };
                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            if (wallet.Balance < billingResult.TotalAmount)
            {
                throw new InvalidOperationException($"Số dư ví không đủ. Số tiền cần thanh toán là {billingResult.TotalAmount:N0}đ nhưng ví của bạn chỉ còn {wallet.Balance:N0}đ. Vui lòng nạp thêm tiền.");
            }

            wallet.Balance -= billingResult.TotalAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var wt = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = -billingResult.TotalAmount,
                Type = WalletTransactionType.Payment,
                Description = $"Thanh toán đặt sân lẻ #{bookingCode}",
                CreatedAt = DateTime.UtcNow
            };
            await _context.WalletTransactions.AddAsync(wt);

            await _bookingRepository.AddAsync(booking);
            await _context.SaveChangesAsync();

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                UserId = booking.UserId,
                CourtId = booking.CourtId,
                CourtName = court.CourtName,
                SlotName = timeSlots.Count > 1 
                    ? $"{CleanSlotName(primarySlot.SlotName)} - {CleanSlotName(timeSlots.Last().SlotName)}" 
                    : CleanSlotName(primarySlot.SlotName),
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
                    ServiceName = bs.Service?.ServiceName ?? string.Empty,
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

        private static string CleanSlotName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            int idx = name.IndexOf('(');
            return idx > 0 ? name.Substring(0, idx).Trim() : name.Trim();
        }
    }
}
