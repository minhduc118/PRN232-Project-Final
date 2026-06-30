using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Home;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
  private readonly IHomeService _homeService;

  public HomeController(IHomeService homeService)
  {
    _homeService = homeService;
  }

  //  GET /api/home
  
  [HttpGet]
  public async Task<IActionResult> GetHomeData()
  {
    var data = await _homeService.GetHomeDataAsync();
    return Ok(ApiResponse<HomeDataDto>.Ok(data,
      "Lấy dữ liệu trang chủ thành công."));
  }
}
