namespace SportCourtManagent_Server.DTOs.Role
{
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserCount { get; set; }
    }

    public class PermissionMatrixRowDto
    {
        public string Feature { get; set; } = string.Empty;
        public bool Admin { get; set; }
        public bool Manager { get; set; }
        public bool Staff { get; set; }
        public bool Customer { get; set; }
    }

    public class AssignRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class SetUserStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class UpdateUserAccessRequest
    {
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
