using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs.Request.Bookings;
using SportCourtManagerment.Models;
using SportCourtManagerment.Repository.Bookings;
using SportCourtManagerment.Services.Courts;
using SportCourtManagerment.Services.Promotions;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly CourtService _courtService;
        private readonly PromotionService _promotionService;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
            _courtService = new CourtService(_context);
            _promotionService = new PromotionService(_context);
        }

        public async Task<bool> CreateBookingAsync(CreateBookingRequestDTO requestDTO, int userId)
        {
            if(requestDTO == null)
            {
                throw new BadHttpRequestException("Request body is null");
            }

            Court? court = await _courtService.GetCourtByIdAsync(requestDTO.CourtId);

            Promotion? promotion = await _promotionService.GetPromotionByIdAsync(requestDTO.PromotionCode);



            Booking newBokings = new Booking
            {
                UserId = userId,
                CourtId = requestDTO.CourtId,
                BookingDate = DateOnly.FromDateTime(DateTime.Now),
                StartTime = TimeOnly.FromDateTime(DateTime.Now),
                EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
                SubTotal = court != null ? court.CourtPricings.FirstOrDefault()?.Price ?? 0 : 0,
                DiscountAmount = promotion != null ? promotion.DiscountValue : 0,
                TotalAmount = court != null ? (court.CourtPricings.FirstOrDefault()?.Price ?? 0) - (promotion != null ? promotion.DiscountValue : 0) : 0,
                PromotionId = promotion?.PromotionId,
                Status = BookingStatus.Pending,
                Note = requestDTO.Note
            };
            throw new NotImplementedException();
        }
    }
}
