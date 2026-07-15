using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleManagementService _roleManagement;

        public RolesController(IRoleManagementService roleManagement)
        {
            _roleManagement = roleManagement ?? throw new ArgumentNullException(nameof(roleManagement));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleManagement.GetAllAsync();
                return Ok(ApiResults.Ok(roles, "Lấy danh sách vai trò thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }

        [HttpGet("permission-matrix")]
        public IActionResult GetPermissionMatrix()
        {
            try
            {
                var rows = _roleManagement.GetPermissionMatrix();
                return Ok(ApiResults.Ok(rows, "Lấy ma trận phân quyền thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResults.Fail(ex.Message, 500));
            }
        }
    }
}
