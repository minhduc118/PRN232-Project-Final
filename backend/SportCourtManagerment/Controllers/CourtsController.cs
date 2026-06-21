using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.DTOs.Reviews;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Controllers;

/// <summary>
/// REST and OData endpoints for court search, detail, availability, and reviews.
/// </summary>
[ApiController]
[Route("api/courts")]
public class CourtsController : ControllerBase
{
  private readonly ICourtService  _courtService;
  private readonly IReviewService _reviewService;

  public CourtsController(ICourtService courtService, IReviewService reviewService)
  {
    _courtService  = courtService;
    _reviewService = reviewService;
  }

  
  //  GET /api/courts — Search with filters + pagination (REST)
  
  [HttpGet]
  public async Task<IActionResult> SearchCourts([FromQuery] CourtSearchParams searchParams)
  {
    var result = await _courtService.SearchCourtsAsync(searchParams);
    return Ok(ApiResponse<PagedResult<CourtListDto>>.Ok(result,
      "Tìm kiếm sân thành công."));
  }

 
  //  GET /api/courts/{id} — Court detail
  
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetCourtDetail(int id)
  {
    var detail = await _courtService.GetCourtDetailAsync(id);
    if (detail is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân.", 404));

    return Ok(ApiResponse<CourtDetailDto>.Ok(detail,
      "Lấy chi tiết sân thành công."));
  }

  
  //  GET /api/courts/{id}/availability?date=YYYY-MM-DD
  
  [HttpGet("{id:int}/availability")]
  public async Task<IActionResult> GetCourtAvailability(
    int id, [FromQuery] DateOnly date)
  {
    var availability = await _courtService.GetCourtAvailabilityAsync(id, date);
    if (availability is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân.", 404));

    return Ok(ApiResponse<CourtAvailabilityDto>.Ok(availability,
      "Lấy lịch trống thành công."));
  }


  //  GET /api/courts/{courtId}/reviews — Court reviews with pagination
  
  [HttpGet("{courtId:int}/reviews")]
  public async Task<IActionResult> GetCourtReviews(
    int courtId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
  {
    var result = await _reviewService.GetCourtReviewsAsync(courtId, pageNumber, pageSize);
    return Ok(ApiResponse<PagedResult<ReviewDto>>.Ok(result,
      "Lấy đánh giá sân thành công."));
  }
}
