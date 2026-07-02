using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Authorization;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.RoleId)
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    UserCount = r.UserRoles.Count
                })
                .ToListAsync();

            return Ok(ApiResults.Ok(roles, "Lấy danh sách vai trò thành công."));
        }

        [HttpGet("permission-matrix")]
        public IActionResult GetPermissionMatrix()
        {
            return Ok(ApiResults.Ok(PermissionMatrix.GetRows(), "Lấy ma trận phân quyền thành công."));
        }
    }
}
