using SportCourtManagerment.Models;

namespace SportCourtManagerment.Repository.Promotions
{
    public interface IPromotionsService
    {
        Task<Promotion?> GetPromotionByIdAsync(string promoCode);
    }
}

