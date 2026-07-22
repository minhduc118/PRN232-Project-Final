using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISePayService _sePayService;

        public WalletController(AppDbContext context, ISePayService sePayService)
        {
            _context = context;
            _sePayService = sePayService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return Unauthorized();
            }

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 10000000m };
                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                walletId = wallet.WalletId,
                userId = wallet.UserId,
                balance = wallet.Balance,
                updatedAt = wallet.UpdatedAt
            });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return Unauthorized();
            }

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                return Ok(Array.Empty<object>());
            }

            var transactions = await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    transactionId = t.TransactionId,
                    amount = t.Amount,
                    type = t.Type.ToString(),
                    bookingId = t.BookingId,
                    description = t.Description,
                    createdAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpGet("deposit-qr")]
        public async Task<IActionResult> GetDepositQr([FromQuery] decimal amount)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return Unauthorized();
            }

            if (amount <= 0)
            {
                return BadRequest(new { message = "Số tiền nạp phải lớn hơn 0." });
            }

            try
            {
                // Generate QR code for wallet deposit WL-{userId}-{amount}
                var qrCode = await _sePayService.GetQrCodeAsync($"WL-{userId}-{amount}");
                return Ok(qrCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("pay-booking")]
        public async Task<IActionResult> PayBooking([FromBody] PayBookingRequest request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(request.BookingCode))
            {
                return BadRequest(new { message = "Mã đặt sân không hợp lệ." });
            }

            var codes = request.BookingCode.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();
            if (codes.Count == 0)
            {
                return BadRequest(new { message = "Không tìm thấy mã đặt sân." });
            }

            var bookings = await _context.Bookings
                .Where(b => codes.Contains(b.BookingCode))
                .ToListAsync();

            if (bookings.Count == 0)
            {
                return BadRequest(new { message = "Không tìm thấy thông tin đặt sân trong cơ sở dữ liệu." });
            }

            decimal totalAmount = bookings.Sum(b => b.TotalAmount);

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 10000000m };
                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            if (wallet.Balance < totalAmount)
            {
                return BadRequest(new { message = $"Số dư ví không đủ. Chi phí thanh toán là {totalAmount:N0}đ nhưng số dư ví chỉ còn {wallet.Balance:N0}đ." });
            }

            // Trừ tiền ví
            wallet.Balance -= totalAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            foreach (var booking in bookings)
            {
                booking.Status = BookingStatus.Confirmed;

                // Ghi nhận Payment nếu chưa có
                var existsPayment = await _context.Payments.AnyAsync(p => p.BookingId == booking.BookingId);
                if (!existsPayment)
                {
                    var payment = new Payment
                    {
                        BookingId = booking.BookingId,
                        Amount = booking.TotalAmount,
                        PaymentMethod = PaymentMethod.Wallet,
                        TransactionId = $"WT-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                        Status = PaymentStatus.Success,
                        PaidAt = DateTime.UtcNow
                    };
                    await _context.Payments.AddAsync(payment);
                }

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
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thanh toán bằng ví thành công." });
        }

        [HttpPost("pay-tournament")]
        public async Task<IActionResult> PayTournament([FromBody] PayTournamentRequest request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
                return Unauthorized();

            var tournament = await _context.Tournaments
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.TournamentId == request.TournamentId);

            if (tournament == null)
                return BadRequest(new { message = "Không tìm thấy giải đấu." });

            if (tournament.UserId != userId)
                return Forbid();

            if (tournament.Status == TournamentStatus.Paid ||
                tournament.Status == TournamentStatus.Confirmed)
                return BadRequest(new { message = "Giải đấu này đã được thanh toán rồi." });

            if (tournament.Status == TournamentStatus.Cancelled)
                return BadRequest(new { message = "Giải đấu đã bị hủy, không thể thanh toán." });

            var totalAmount = tournament.TotalAmount;

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 10000000m };
                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            if (wallet.Balance < totalAmount)
                return BadRequest(new { message = $"Số dư ví không đủ. Chi phí giải đấu là {totalAmount:N0}đ nhưng ví chỉ còn {wallet.Balance:N0}đ." });

            // Trừ tiền ví
            wallet.Balance -= totalAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Cập nhật trạng thái giải đấu
            tournament.Status = TournamentStatus.Paid;

            // Cập nhật trạng thái các Booking trong giải đấu
            foreach (var booking in tournament.Bookings)
            {
                booking.Status = BookingStatus.Confirmed;

                var existsPayment = await _context.Payments.AnyAsync(p => p.BookingId == booking.BookingId);
                if (!existsPayment)
                {
                    await _context.Payments.AddAsync(new Payment
                    {
                        BookingId = booking.BookingId,
                        Amount = booking.TotalAmount,
                        PaymentMethod = PaymentMethod.Wallet,
                        TransactionId = $"WT-TM-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                        Status = PaymentStatus.Success,
                        PaidAt = DateTime.UtcNow
                    });
                }
            }

            // Ghi WalletTransaction cho giải đấu
            await _context.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = -totalAmount,
                Type = WalletTransactionType.Payment,
                Description = $"Thanh toán giải đấu #{tournament.TournamentId} - {tournament.TournamentName}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thanh toán giải đấu bằng ví thành công." });
        }

        [HttpPost("pay-services")]
        public async Task<IActionResult> PayServices([FromBody] PayServicesRequest request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return Unauthorized();
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingCode == request.BookingCode);
            if (booking == null)
            {
                return BadRequest(new { message = "Không tìm thấy đặt sân." });
            }

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 10000000m };
                await _context.Wallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            if (wallet.Balance < request.Amount)
            {
                return BadRequest(new { message = $"Số dư ví không đủ. Chi phí thanh toán dịch vụ bổ sung là {request.Amount:N0}đ nhưng ví chỉ còn {wallet.Balance:N0}đ." });
            }

            // Trừ tiền ví
            wallet.Balance -= request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Ghi nhận Payment cho dịch vụ bổ sung
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = request.Amount,
                PaymentMethod = PaymentMethod.Wallet,
                TransactionId = $"WT-SRV-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                Status = PaymentStatus.Success,
                PaidAt = DateTime.UtcNow
            };
            await _context.Payments.AddAsync(payment);

            // Ghi nhận WalletTransaction
            var wt = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = -request.Amount,
                Type = WalletTransactionType.Payment,
                BookingId = booking.BookingId,
                Description = $"Thanh toán dịch vụ bổ sung cho đặt sân {booking.BookingCode}",
                CreatedAt = DateTime.UtcNow
            };
            await _context.WalletTransactions.AddAsync(wt);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thanh toán dịch vụ bổ sung thành công." });
        }
    }

    public class PayBookingRequest
    {
        public string BookingCode { get; set; } = "";
    }

    public class PayServicesRequest
    {
        public string BookingCode { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class PayTournamentRequest
    {
        public int TournamentId { get; set; }
    }
}
