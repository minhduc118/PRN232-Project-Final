using SportCourtManagerment.Data;
using SportCourtManagerment.Models;
using SportCourtManagerment.Repository.Courts;

namespace SportCourtManagerment.Services.Courts
{
    public class CourtService : ICourtService
    {
        private readonly ApplicationDbContext _context;

        public CourtService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Court?> GetCourtByIdAsync(int courtId)
        {
            if (courtId <= 0)
            {
                throw new BadHttpRequestException("Thông tin ID của sân không hợp lệ.");
            }

            try
            {
                var court = await _context.Courts.FindAsync(courtId);
                if (court == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sân với ID {courtId}.");
                }
                return court;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the court: {ex.Message}");
                throw new BadHttpRequestException("Đã xảy ra lỗi trong quá trình lấy thông tin court.", 400);
            }
        }
    }
}
