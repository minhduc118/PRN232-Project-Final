using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}
