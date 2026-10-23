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

    [HttpPost("check-expirations")]
    public async Task<IActionResult> CheckExpirations([FromServices] ISoftwareNotificationService notificationService)
    {
        var count = await notificationService.CheckAndSendSoftwareExpiryAlertsAsync();
        return Ok(new 
        { 
            message = count > 0 
                ? $"Software expiration check completed. {count} notification email(s) sent." 
                : "Software expiration check completed. No subscriptions expiring within 7 days.",
            count 
        });
    }

    [HttpPost("{id}/send-reminder")]
    public async Task<IActionResult> SendReminder(string id, [FromServices] ISoftwareNotificationService notificationService)
    {
        var success = await notificationService.SendSingleSoftwareReminderAsync(id);
        if (!success)
        {
            return NotFound(new { message = $"Software subscription with ID '{id}' was not found." });
        }
        return Ok(new { message = "Software reminder email sent successfully to management and responsible person." });
    }
}
