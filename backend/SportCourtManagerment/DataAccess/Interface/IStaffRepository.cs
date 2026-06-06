using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Interface;

public interface IStaffRepository
{
  Task<(List<User> Items, int TotalCount)> GetStaffByComplexAsync(
    int     complexId,
    string? search   = null,
    bool?   isActive = null,
    int     page     = 1,
    int     pageSize = 20);

  Task<bool> IsStaffOfComplexAsync(int staffId, int complexId);

  Task<User?> GetStaffWithRolesAsync(int staffId);
}
