using AssetHub.Core.Dtos.Maintenance;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _service;

    public MaintenanceController(IMaintenanceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceRecordDto>>> GetAll([FromQuery] string? assetId, [FromQuery] string? status) =>
        Ok(await _service.GetAllAsync(assetId, status));

    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceRecordDto>> GetById(string id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Opens a ticket and flips the asset to "Under Repair" in one call.</summary>
    [HttpPost]
    public async Task<ActionResult<MaintenanceRecordDto>> Raise([FromBody] RaiseMaintenanceDto dto)
    {
        var created = await _service.RaiseAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Closes a ticket and restores the asset's status.</summary>
    [HttpPost("{id}/close")]
    public async Task<ActionResult<MaintenanceRecordDto>> Close(string id, [FromBody] CloseMaintenanceDto dto) =>
        Ok(await _service.CloseAsync(id, dto));
}
