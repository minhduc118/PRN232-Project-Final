using System.Collections.Generic;
using SportCourtManagent_Server.DTOs.Role;

namespace SportCourtManagent_Server.Authorization
{
    /// <summary>Ma trận phân quyền theo SRS FE-10. Cột Manager = quản lý tổ hợp (Staff được gán ManagerId).</summary>
    public static class PermissionMatrix
    {
        private static List<PermissionMatrixRowDto> _rows =
        [
            new() { Feature = "Quản lý sân", Admin = true, Manager = true, Staff = false, Customer = false },
            new() { Feature = "Quản lý đặt sân", Admin = true, Manager = true, Staff = true, Customer = true },
            new() { Feature = "Quản lý khách hàng", Admin = true, Manager = true, Staff = true, Customer = false },
            new() { Feature = "Thống kê doanh thu", Admin = true, Manager = true, Staff = false, Customer = false },
            new() { Feature = "Quản lý dịch vụ", Admin = true, Manager = true, Staff = true, Customer = false },
            new() { Feature = "Đặt sân", Admin = true, Manager = true, Staff = true, Customer = true },
            new() { Feature = "Đánh giá", Admin = false, Manager = false, Staff = false, Customer = true },
            new() { Feature = "Quản lý khuyến mãi", Admin = true, Manager = false, Staff = false, Customer = false },
            new() { Feature = "Quản lý nhân viên", Admin = true, Manager = true, Staff = false, Customer = false },
            new() { Feature = "Phân quyền hệ thống", Admin = true, Manager = false, Staff = false, Customer = false },
        ];

        public static IReadOnlyList<PermissionMatrixRowDto> GetRows() => _rows;

        public static void UpdateRows(List<PermissionMatrixRowDto> newRows)
        {
            if (newRows != null && newRows.Count > 0)
            {
                _rows = newRows;
            }
        }

        public static readonly HashSet<string> ValidRoleNames =
        [
            "Admin", "Manager", "Staff", "Customer"
        ];
    }
}
