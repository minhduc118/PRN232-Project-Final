using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs.Membership;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipTiersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembershipTiersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTiers()
        {
            var tiers = await _context.MembershipTiers
                .OrderBy(t => t.MinPoints)
                .Select(t => new MembershipTierDto
                {
                    TierId = t.TierId,
                    TierName = t.TierName,
                    MinPoints = t.MinPoints,
                    DiscountPercent = t.DiscountPercent
                })
                .ToListAsync();

            return Ok(new { data = tiers });
        }
    }
}
