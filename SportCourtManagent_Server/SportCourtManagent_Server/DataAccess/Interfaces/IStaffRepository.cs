using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IStaffRepository
    {
        Task<(List<User> Items, int TotalCount)> GetStaffByComplexAsync(int complexId, string? search = null, bool? isActive = null, int page = 1, int pageSize = 10);
        Task<bool> IsStaffOfComplexAsync(int staffId, int complexId);
        Task<User?> GetStaffWithRolesAsync(int staffId);
        Task AssignStaffToComplexAsync(int staffId, int complexId);
        Task RemoveStaffFromComplexAsync(int staffId, int complexId);
        Task<List<User>> GetUnassignedStaffsAsync();
    }
}
