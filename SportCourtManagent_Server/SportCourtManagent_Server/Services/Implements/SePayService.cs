using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.DTOs.Payments;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class SePayService : ISePayService
    {
        private readonly IConfiguration _configuration;
        private readonly IInMemoryBookingRepository _inMemoryBookingRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtBookingService _courtBookingService;

        public SePayService(
            IConfiguration configuration,
            IInMemoryBookingRepository inMemoryBookingRepository,
            IBookingRepository bookingRepository,
            ICourtBookingService courtBookingService)
        {
            _configuration = configuration;
            _inMemoryBookingRepository = inMemoryBookingRepository;
            _bookingRepository = bookingRepository;
            _courtBookingService = courtBookingService;
        }

        public async Task<SePayQrCodeResponse> GetQrCodeAsync(string bookingCode)
        {
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

            var bankBin = _configuration["SePay:BankBin"] ?? "970422";
            var accountNumber = _configuration["SePay:AccountNumber"] ?? "101830532104";
            var accountName = _configuration["SePay:AccountName"] ?? "NGUYEN VU KIM PHUOC";

            string encodedAccountName = Uri.EscapeDataString(accountName);
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

            // 3. Extract Booking Code
            var match = Regex.Match(payload.Content, @"BK-[A-Z0-9]{8}", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = Regex.Match(payload.Description, @"BK-[A-Z0-9]{8}", RegexOptions.IgnoreCase);
            }

            if (!match.Success)
            {
                throw new ArgumentException("No valid booking code found in transfer content.");
            }

            string bookingCode = match.Value.ToUpper();

            // 4. Retrieve booking
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

            // 5. Verify amount
            if (payload.TransferAmount < booking.TotalAmount)
            {
                throw new ArgumentException($"Transferred amount ({payload.TransferAmount}) is less than booking total ({booking.TotalAmount}).");
            }

            // 6. Confirm payment via CourtBookingService
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
