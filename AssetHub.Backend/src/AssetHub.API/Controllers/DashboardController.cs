using AssetHub.Core.Dtos.Dashboard;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    /// <summary>GET /api/dashboard/summary — one call for every Dashboard.jsx stat card, chart and
    /// the recent-activity feed, instead of the frontend deriving them client-side from raw lists.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary() => Ok(await _service.GetSummaryAsync());
}
