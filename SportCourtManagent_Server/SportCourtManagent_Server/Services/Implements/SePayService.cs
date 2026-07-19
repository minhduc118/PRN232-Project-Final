using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.DTOs.Payments;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class SePayService : ISePayService
    {
        private readonly IConfiguration _configuration;
        private readonly IInMemoryBookingRepository _inMemoryBookingRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtBookingService _courtBookingService;
        private readonly AppDbContext _context;

        public SePayService(
            IConfiguration configuration,
            IInMemoryBookingRepository inMemoryBookingRepository,
            IBookingRepository bookingRepository,
            ICourtBookingService courtBookingService,
            AppDbContext context)
        {
            _configuration = configuration;
            _inMemoryBookingRepository = inMemoryBookingRepository;
            _bookingRepository = bookingRepository;
            _courtBookingService = courtBookingService;
            _context = context;
        }

        public async Task<SePayQrCodeResponse> GetQrCodeAsync(string bookingCode)
        {
            if (string.IsNullOrWhiteSpace(bookingCode))
            {
                throw new ArgumentException("Mã thanh toán không được để trống.");
            }

            var bankBin = _configuration["SePay:BankBin"] ?? "970422";
            var accountNumber = _configuration["SePay:AccountNumber"] ?? "101830532104";
            var accountName = _configuration["SePay:AccountName"] ?? "NGUYEN VU KIM PHUOC";
            string encodedAccountName = Uri.EscapeDataString(accountName);

            // Xử lý mã thanh toán giải đấu (TM-{tournamentId})
            if (bookingCode.StartsWith("TM-", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(bookingCode.Substring(3), out int tournamentId))
                {
                    var tour = await _context.Tournaments.FindAsync(tournamentId);
                    if (tour == null) throw new KeyNotFoundException($"Không tìm thấy giải đấu #{tournamentId}.");
                    if (tour.Status == TournamentStatus.Paid || tour.Status == TournamentStatus.Confirmed)
                    {
                        throw new InvalidOperationException("Giải đấu đã được xác nhận/thanh toán trước đó.");
                    }
                    if (tour.Status == TournamentStatus.Cancelled || (tour.Status == TournamentStatus.Pending && tour.ExpiredAt.HasValue && tour.ExpiredAt.Value < DateTime.UtcNow))
                    {
                        throw new InvalidOperationException("Giải đấu đã hết hạn giữ chỗ hoặc đã bị hủy.");
                    }

                    string tourQrUrl = $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact.png?amount={tour.TotalAmount}&addInfo={bookingCode.ToUpper()}&accountName={encodedAccountName}";
                    return new SePayQrCodeResponse
                    {
                        BookingCode = bookingCode.ToUpper(),
                        Amount = tour.TotalAmount,
                        BankBin = bankBin,
                        AccountNumber = accountNumber,
                        AccountName = accountName,
                        Description = $"Thanh toán giải đấu #{tournamentId}",
                        QrCodeUrl = tourQrUrl
                    };
                }
            }

            var booking = await _inMemoryBookingRepository.GetByCodeAsync(bookingCode);
            decimal amount;
            string code;
            if (booking == null)
            {
                var dbBooking = (await _bookingRepository.GetAllAsync()).FirstOrDefault(b => string.Equals(b.BookingCode, bookingCode, StringComparison.OrdinalIgnoreCase));
                if (dbBooking != null)
                {
                    amount = dbBooking.TotalAmount;
                    code = dbBooking.BookingCode;
                }
                else
                {
                    throw new KeyNotFoundException("Booking session has expired or does not exist.");
                }
            }
            else
            {
                amount = booking.TotalAmount;
                code = booking.BookingCode;
            }

            string qrUrl = $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact.png?amount={amount}&addInfo={code}&accountName={encodedAccountName}";

            return new SePayQrCodeResponse
            {
                BookingCode = code,
                Amount = amount,
                BankBin = bankBin,
                AccountNumber = accountNumber,
                AccountName = accountName,
                Description = code,
                QrCodeUrl = qrUrl
            };
        }

        public async Task<BookingResponseDto> HandleWebhookAsync(SePayWebhookPayload payload, string? authHeader)
        {
            // 1. Verify webhook authorization API Key
            if (string.IsNullOrEmpty(authHeader))
            {
                throw new UnauthorizedAccessException("Missing Authorization header.");
            }

            string authValue = authHeader.Trim();
            string expectedApiKey = _configuration["SePay:WebhookApiKey"] ?? string.Empty;

            if (string.IsNullOrEmpty(expectedApiKey))
            {
                throw new InvalidOperationException("Webhook API Key is not configured on the server.");
            }

            if (authValue != expectedApiKey && authValue != $"Apikey {expectedApiKey}")
            {
                throw new UnauthorizedAccessException("Invalid Authorization API Key.");
            }

            // 2. Verify transfer type
            if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Ignored: Not an incoming payment.");
            }

            // 3. Extract Tournament Code (TM-xxx)
            var tourMatch = Regex.Match(payload.Content, @"TM-?[0-9]+", RegexOptions.IgnoreCase);
            if (!tourMatch.Success) tourMatch = Regex.Match(payload.Description, @"TM-?[0-9]+", RegexOptions.IgnoreCase);

            if (tourMatch.Success)
            {
                string rawTour = tourMatch.Value.ToUpper();
                string tourCode = rawTour.StartsWith("TM-") ? rawTour : "TM-" + rawTour.Substring(2);
                if (int.TryParse(tourCode.Substring(3), out int tournamentId))
                {
                    var tour = await _context.Tournaments
                        .Include(t => t.Bookings)
                        .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
                    if (tour == null) throw new KeyNotFoundException($"Không tìm thấy giải đấu #{tournamentId}.");
                    if (tour.Status == TournamentStatus.Paid || tour.Status == TournamentStatus.Confirmed)
                    {
                        throw new InvalidOperationException("Giải đấu đã được thanh toán trước đó.");
                    }
                    if (tour.Status == TournamentStatus.Cancelled)
                    {
                        throw new InvalidOperationException("Giải đấu này đã bị hủy.");
                    }
                    if (payload.TransferAmount < tour.TotalAmount)
                    {
                        throw new ArgumentException($"Số tiền chuyển ({payload.TransferAmount:N0}đ) nhỏ hơn tổng chi phí giải đấu ({tour.TotalAmount:N0}đ).");
                    }

                    tour.Status = TournamentStatus.Paid;
                    string transactionRef = !string.IsNullOrEmpty(payload.ReferenceCode) ? payload.ReferenceCode : payload.Id.ToString();
                    foreach (var b in tour.Bookings.Where(b => b.Status == BookingStatus.Pending))
                    {
                        b.Status = BookingStatus.Confirmed;
                        _context.Payments.Add(new Payment
                        {
                            BookingId = b.BookingId,
                            Amount = b.TotalAmount,
                            PaymentMethod = PaymentMethod.BankTransfer,
                            TransactionId = $"{transactionRef}-B{b.BookingId}",
                            Status = PaymentStatus.Success,
                            PaidAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();

                    return new BookingResponseDto
                    {
                        BookingId = tour.TournamentId,
                        BookingCode = tourCode,
                        CourtName = tour.TournamentName,
                        BookingDate = tour.CreatedAt,
                        TotalAmount = tour.TotalAmount,
                        Status = "Paid"
                    };
                }
            }

            // 4. Extract Booking Code (supports BK-XXXXXXXX and BK2026MMDD...)
            var match = Regex.Match(payload.Content, @"BK-?[A-Z0-9]{8,20}", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = Regex.Match(payload.Description, @"BK-?[A-Z0-9]{8,20}", RegexOptions.IgnoreCase);
            }

            if (!match.Success)
            {
                throw new ArgumentException("No valid booking code found in transfer content.");
            }

            string rawCode = match.Value.ToUpper();
            string code1 = rawCode;
            string code2 = rawCode.StartsWith("BK-") 
                ? rawCode 
                : (rawCode.StartsWith("BK") && rawCode.Length > 2 ? "BK-" + rawCode.Substring(2) : rawCode);

            // 5. Retrieve booking (checking both code formats)
            string bookingCode = code1;
            var booking = await _inMemoryBookingRepository.GetByCodeAsync(code1);
            if (booking == null)
            {
                booking = await _inMemoryBookingRepository.GetByCodeAsync(code2);
                if (booking != null)
                {
                    bookingCode = code2;
                }
            }

            if (booking == null)
            {
                var dbBooking = (await _bookingRepository.GetAllAsync())
                    .FirstOrDefault(b => string.Equals(b.BookingCode, code1, StringComparison.OrdinalIgnoreCase) 
                                      || string.Equals(b.BookingCode, code2, StringComparison.OrdinalIgnoreCase));
                if (dbBooking != null)
                {
                    if (dbBooking.Status == BookingStatus.Confirmed)
                    {
                        var paymentRecord = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == dbBooking.BookingId);
                        if (paymentRecord == null || paymentRecord.Amount < dbBooking.TotalAmount)
                        {
                            decimal remainingBalance = dbBooking.TotalAmount - (paymentRecord?.Amount ?? 0);
                            if (payload.TransferAmount < remainingBalance)
                            {
                                throw new ArgumentException($"Transferred amount ({payload.TransferAmount}) is less than the remaining service balance ({remainingBalance}).");
                            }

                            if (paymentRecord != null)
                            {
                                paymentRecord.Amount = dbBooking.TotalAmount;
                                paymentRecord.TransactionId = !string.IsNullOrEmpty(payload.ReferenceCode) ? payload.ReferenceCode : payload.Id.ToString();
                                paymentRecord.PaidAt = DateTime.UtcNow;
                                _context.Payments.Update(paymentRecord);
                            }
                            else
                            {
                                var payment = new Payment
                                {
                                    BookingId = dbBooking.BookingId,
                                    Amount = dbBooking.TotalAmount,
                                    PaymentMethod = PaymentMethod.BankTransfer,
                                    TransactionId = !string.IsNullOrEmpty(payload.ReferenceCode) ? payload.ReferenceCode : payload.Id.ToString(),
                                    Status = PaymentStatus.Success,
                                    PaidAt = DateTime.UtcNow
                                };
                                await _context.Payments.AddAsync(payment);
                            }
                            await _context.SaveChangesAsync();

                            var court = await _context.Courts.FindAsync(dbBooking.CourtId);
                            var slot = await _context.TimeSlots.FindAsync(dbBooking.SlotId);

                            return new BookingResponseDto
                            {
                                BookingId = dbBooking.BookingId,
                                BookingCode = dbBooking.BookingCode,
                                UserId = dbBooking.UserId,
                                CourtId = dbBooking.CourtId,
                                CourtName = court?.CourtName ?? "Sân đấu",
                                SlotId = dbBooking.SlotId,
                                SlotName = slot?.SlotName ?? "Khung giờ",
                                BookingDate = dbBooking.BookingDate,
                                StartTime = dbBooking.StartTime,
                                EndTime = dbBooking.EndTime,
                                SubTotal = dbBooking.SubTotal,
                                DiscountAmount = dbBooking.DiscountAmount,
                                TotalAmount = dbBooking.TotalAmount,
                                Status = dbBooking.Status.ToString()
                            };
                        }
                        throw new InvalidOperationException("Booking has already been confirmed and paid.");
                    }

                    if (dbBooking.Status == BookingStatus.Pending)
                    {
                        // Verify amount
                        if (payload.TransferAmount < dbBooking.TotalAmount)
                        {
                            throw new ArgumentException($"Transferred amount ({payload.TransferAmount}) is less than booking total ({dbBooking.TotalAmount}).");
                        }

                        // Confirm payment for database booking
                        dbBooking.Status = BookingStatus.Confirmed;
                        
                        var payment = new Payment
                        {
                            BookingId = dbBooking.BookingId,
                            Amount = dbBooking.TotalAmount,
                            PaymentMethod = PaymentMethod.BankTransfer,
                            TransactionId = !string.IsNullOrEmpty(payload.ReferenceCode) ? payload.ReferenceCode : payload.Id.ToString(),
                            Status = PaymentStatus.Success,
                            PaidAt = DateTime.UtcNow
                        };
                        await _context.Payments.AddAsync(payment);
                        await _context.SaveChangesAsync();

                        var court = await _context.Courts.FindAsync(dbBooking.CourtId);
                        var slot = await _context.TimeSlots.FindAsync(dbBooking.SlotId);

                        return new BookingResponseDto
                        {
                            BookingId = dbBooking.BookingId,
                            BookingCode = dbBooking.BookingCode,
                            UserId = dbBooking.UserId,
                            CourtId = dbBooking.CourtId,
                            CourtName = court?.CourtName ?? "Sân đấu",
                            SlotId = dbBooking.SlotId,
                            SlotName = slot?.SlotName ?? "Khung giờ",
                            BookingDate = dbBooking.BookingDate,
                            StartTime = dbBooking.StartTime,
                            EndTime = dbBooking.EndTime,
                            SubTotal = dbBooking.SubTotal,
                            DiscountAmount = dbBooking.DiscountAmount,
                            TotalAmount = dbBooking.TotalAmount,
                            Status = dbBooking.Status.ToString()
                        };
                    }

                    throw new InvalidOperationException($"Booking found in database but has status: {dbBooking.Status}.");
                }
                throw new KeyNotFoundException($"Booking {rawCode} session has expired or does not exist.");
            }

            // 6. Verify amount
            if (payload.TransferAmount < booking.TotalAmount)
            {
                throw new ArgumentException($"Transferred amount ({payload.TransferAmount}) is less than booking total ({booking.TotalAmount}).");
            }

            // 7. Confirm payment via CourtBookingService
            var confirmDto = new ConfirmPaymentRequestDto
            {
                BookingCode = bookingCode,
                PaymentMethod = "BankTransfer",
                TransactionId = !string.IsNullOrEmpty(payload.ReferenceCode) ? payload.ReferenceCode : payload.Id.ToString()
            };

            return await _courtBookingService.ConfirmPaymentAsync(confirmDto);
        }
    }
}
