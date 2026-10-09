using AssetHub.Core.Dtos.Software;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SoftwareController : ControllerBase
{
    private readonly ISoftwareService _service;

    public SoftwareController(ISoftwareService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<SoftwareSubscriptionDto>>> GetAll([FromQuery] string? category, [FromQuery] string? status) =>
        Ok(await _service.GetAllAsync(category, status));

    [HttpGet("{id}")]
    public async Task<ActionResult<SoftwareSubscriptionDto>> GetById(string id) =>
        Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<SoftwareSubscriptionDto>> Create([FromBody] CreateSoftwareSubscriptionDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SoftwareSubscriptionDto>> Update(string id, [FromBody] UpdateSoftwareSubscriptionDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
