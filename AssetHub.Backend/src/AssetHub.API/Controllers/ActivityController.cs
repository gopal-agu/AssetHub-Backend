using AssetHub.Core.Dtos.Dashboard;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivityController : ControllerBase
{
  private readonly IActivityLogService _activityLogService;

  public ActivityController(IActivityLogService activityLogService)
  {
    _activityLogService = activityLogService;
  }

  /// <summary>GET /api/activity — recent activity feed, matches the frontend's api.activity.getAll().</summary>
  [HttpGet]
  public async Task<ActionResult<List<ActivityLogDto>>> GetAll([FromQuery] int take = 40) =>
      Ok(await _activityLogService.GetRecentAsync(take));

  /// <summary>POST /api/activity — lets the frontend append its own log entries (api.activity.log({ text })),
  /// on top of the automatic ones every service already writes internally on create/update/assign/etc.</summary>
  [HttpPost]
  public async Task<IActionResult> Log([FromBody] LogActivityRequestDto dto)
  {
    if (string.IsNullOrWhiteSpace(dto?.Text)) return BadRequest(new { message = "Text is required." });
    await _activityLogService.LogAsync(dto.Text);
    return NoContent();
  }
}
