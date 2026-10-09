using AssetHub.Core.Dtos.Vendor;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
  private readonly IVendorService _service;

  public VendorsController(IVendorService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<ActionResult<List<VendorDto>>> GetAll() => Ok(await _service.GetAllAsync());

  [HttpGet("{id}")]
  public async Task<ActionResult<VendorDto>> GetById(string id) => Ok(await _service.GetByIdAsync(id));

  [HttpPost]
  public async Task<ActionResult<VendorDto>> Create([FromBody] CreateVendorDto dto)
  {
    var created = await _service.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<VendorDto>> Update(string id, [FromBody] UpdateVendorDto dto) =>
      Ok(await _service.UpdateAsync(id, dto));

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
    await _service.DeleteAsync(id);
    return NoContent();
  }
}
