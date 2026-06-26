using SportCourtManagerment.Models;

namespace SportCourtManagerment.Repository.Courts
{
    public interface ICourtService
    {
        Task<Court?> GetCourtByIdAsync(int courtId);
    }
}
