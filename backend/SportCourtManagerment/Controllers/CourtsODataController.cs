using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Controllers;

/// <summary>
/// OData-enabled endpoint for flexible court queries.
/// Supports $filter, $orderby, $top, $skip, $select, $count.
/// Example: GET /odata/courts?$filter=CourtTypeId eq 3&amp;$orderby=CourtName&amp;$top=5
/// </summary>
[Route("odata")]
public class CourtsODataController : ODataController
{
  private readonly ICourtService _courtService;

  public CourtsODataController(ICourtService courtService)
  {
    _courtService = courtService;
  }

  [HttpGet("courts")]
  [EnableQuery(MaxTop = 100, PageSize = 50)]
  public IActionResult Get()
  {
    return Ok(_courtService.GetCourtsODataQueryable());
  }
}
