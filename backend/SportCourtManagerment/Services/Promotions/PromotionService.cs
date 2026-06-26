using SportCourtManagerment.Data;
using SportCourtManagerment.Models;
using SportCourtManagerment.Repository.Promotions;

namespace SportCourtManagerment.Services.Promotions
{
    public class PromotionService : IPromotionsService
    {
        private readonly ApplicationDbContext _context;

        public PromotionService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Promotion?> GetPromotionByIdAsync(string promoCode)
        {
            if (string.IsNullOrEmpty(promoCode))
            {
                return null;
            }

            try
            {
                var promotion = await _context.Promotions.FindAsync(promoCode);
                return promotion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi : {ex.Message}");
                throw new BadHttpRequestException("Mã khuyến mãi không hợp lệ hoặc không tồn tại.");
            }
        }
    }
}
