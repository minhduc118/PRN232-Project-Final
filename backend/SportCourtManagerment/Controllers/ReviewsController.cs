using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using SportCourtManagerment.DTOs.Reviews;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Controllers;


[Route("odata")]
public class ReviewsODataController : ODataController
{
  private readonly IReviewService _reviewService;

  public ReviewsODataController(IReviewService reviewService)
  {
    _reviewService = reviewService;
  }

  [HttpGet("reviews")]
  [EnableQuery(MaxTop = 100, PageSize = 50)]
  public IActionResult Get()
  {
    return Ok(_reviewService.GetReviewsODataQueryable());
  }
}
