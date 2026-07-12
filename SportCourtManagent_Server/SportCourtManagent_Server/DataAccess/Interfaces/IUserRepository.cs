using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByIdWithDetailsAsync(int id);
        Task<User?> GetByEmailWithDetailsAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByPhoneAsync(string phone);
        Task<IReadOnlyList<User>> GetPagedWithDetailsAsync(
            string? search,
            string? role,
            bool? isActive,
            int page,
            int pageSize);
        Task<int> CountAsync(string? search, string? role, bool? isActive);
        Task AddAsync(User entity);
        Task UpdateAsync(User entity);
        Task SaveChangesAsync();
    }
}
