using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Review;
using SportCourtManagent_Server.Services.Interfaces;
using SportCourtManagent_Server.Helpers;


namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/courts/{courtId:int}/reviews")]
    public class ReviewsController : ODataController
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET /api/courts/{courtId}/reviews — Court reviews with pagination
        [HttpGet]
        public async Task<IActionResult> GetCourtReviews(
            int courtId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reviewService.GetCourtReviewsAsync(courtId, pageNumber, pageSize);
            return Ok(ApiResults.Ok(result));

        }

        // POST /api/courts/{courtId}/reviews — Create a new review (requires authentication)
        [HttpPost]
        [Authorize(Roles="Customer")]
        public async Task<IActionResult> CreateReview(
            int courtId,
            [FromBody] CreateReviewDto dto)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResults.Fail("Token không hợp lệ hoặc không xác định được người dùng.", 401));


            var (review, error) = await _reviewService.CreateReviewAsync(courtId, userId, dto);

            if (error is not null)
                return BadRequest(ApiResults.Fail(error, 400));

            return StatusCode(201, ApiResults.Ok(review, "Tạo đánh giá thành công.", 201));

        }

        // GET /odata/reviews — OData query
        [HttpGet("/odata/reviews")]
        [EnableQuery(MaxTop = 100, PageSize = 50)]
        public IActionResult GetOData()
        {
            return Ok(_reviewService.GetReviewsODataQueryable());
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst("sub")?.Value;
            return !string.IsNullOrEmpty(claim) && int.TryParse(claim, out userId);
        }
    }
}
