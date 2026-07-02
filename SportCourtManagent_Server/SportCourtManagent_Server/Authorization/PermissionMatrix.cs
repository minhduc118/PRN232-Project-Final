using System.Collections.Generic;
using SportCourtManagent_Server.DTOs.Role;

namespace SportCourtManagent_Server.Authorization
{
    /// <summary>Ma trận phân quyền theo SRS FE-10. Cột Manager = quản lý tổ hợp (Staff được gán ManagerId).</summary>
    public static class PermissionMatrix
    {
        public static IReadOnlyList<PermissionMatrixRowDto> GetRows() =>
        [
            new() { Feature = "Quản lý sân", Admin = true, Manager = true, Staff = false, Coach = false, Customer = false },
            new() { Feature = "Quản lý đặt sân", Admin = true, Manager = true, Staff = true, Coach = false, Customer = true },
            new() { Feature = "Quản lý khách hàng", Admin = true, Manager = true, Staff = true, Coach = false, Customer = false },
            new() { Feature = "Thống kê doanh thu", Admin = true, Manager = true, Staff = false, Coach = false, Customer = false },
            new() { Feature = "Quản lý dịch vụ", Admin = true, Manager = true, Staff = true, Coach = false, Customer = false },
            new() { Feature = "Quản lý lịch dạy", Admin = true, Manager = true, Staff = false, Coach = true, Customer = false },
            new() { Feature = "Đặt sân", Admin = true, Manager = true, Staff = true, Coach = true, Customer = true },
            new() { Feature = "Đánh giá", Admin = false, Manager = false, Staff = false, Coach = false, Customer = true },
            new() { Feature = "Quản lý khuyến mãi", Admin = true, Manager = false, Staff = false, Coach = false, Customer = false },
            new() { Feature = "Quản lý nhân viên", Admin = true, Manager = true, Staff = false, Coach = false, Customer = false },
            new() { Feature = "Phân quyền hệ thống", Admin = true, Manager = false, Staff = false, Coach = false, Customer = false },
        ];

        public static readonly HashSet<string> ValidRoleNames =
        [
            "Admin", "Staff", "Coach", "Customer"
        ];
    }
}
