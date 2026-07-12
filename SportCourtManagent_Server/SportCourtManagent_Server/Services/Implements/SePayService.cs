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
            if (booking == null)
            {
                var dbBooking = (await _bookingRepository.GetAllAsync()).FirstOrDefault(b => b.BookingCode == bookingCode);
                if (dbBooking != null)
                {
                    throw new InvalidOperationException($"Booking is already {dbBooking.Status}.");
                }
                throw new KeyNotFoundException("Booking session has expired or does not exist.");
            }

            string qrUrl = $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact.png?amount={booking.TotalAmount}&addInfo={booking.BookingCode}&accountName={encodedAccountName}";

            return new SePayQrCodeResponse
            {
                BookingCode = booking.BookingCode,
                Amount = booking.TotalAmount,
                BankBin = bankBin,
                AccountNumber = accountNumber,
                AccountName = accountName,
                Description = booking.BookingCode,
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
                    if (payload.TransferAmount < tour.TotalAmount)
                    {
                        throw new ArgumentException($"Số tiền chuyển ({payload.TransferAmount:N0}đ) nhỏ hơn tổng chi phí giải đấu ({tour.TotalAmount:N0}đ).");
                    }

                    tour.Status = TournamentStatus.Paid;
                    foreach (var b in tour.Bookings.Where(b => b.Status == BookingStatus.Pending))
                    {
                        b.Status = BookingStatus.Confirmed;
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

            // 4. Extract Booking Code (BK-xxxxxxxx)
            var match = Regex.Match(payload.Content, @"BK-?[A-Z0-9]{8}", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = Regex.Match(payload.Description, @"BK-?[A-Z0-9]{8}", RegexOptions.IgnoreCase);
            }

            if (!match.Success)
            {
                throw new ArgumentException("No valid booking code found in transfer content.");
            }

            string rawCode = match.Value.ToUpper();
            string bookingCode = rawCode.StartsWith("BK-") ? rawCode : "BK-" + rawCode.Substring(2);

            // 5. Retrieve booking
            var booking = await _inMemoryBookingRepository.GetByCodeAsync(bookingCode);
            if (booking == null)
            {
                var dbBooking = (await _bookingRepository.GetAllAsync()).FirstOrDefault(b => b.BookingCode == bookingCode);
                if (dbBooking != null)
                {
                    if (dbBooking.Status == BookingStatus.Confirmed)
                    {
                        throw new InvalidOperationException("Booking has already been confirmed and paid.");
                    }
                    throw new InvalidOperationException($"Booking found in database but has status: {dbBooking.Status}.");
                }
                throw new KeyNotFoundException($"Booking {bookingCode} session has expired or does not exist.");
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
