//using AssetHub.Core.Dtos.Inventory;
//using AssetHub.Core.Interfaces.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace AssetHub.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[Authorize]
//public class InventoryController : ControllerBase
//{
//    private readonly IInventoryService _service;

//    public InventoryController(IInventoryService service)
//    {
//        _service = service;
//    }

//    [HttpGet]
//    public async Task<ActionResult<List<InventoryBatchDto>>> GetAll() => Ok(await _service.GetAllAsync());

//    [HttpGet("{id}")]
//    public async Task<ActionResult<InventoryBatchDto>> GetById(string id) => Ok(await _service.GetByIdAsync(id));

//    /// <summary>Records one intake. If configuration values are supplied, also bulk-generates the
//    /// individual Asset rows for the batch's quantity — see InventoryService.CreateAsync.</summary>
//    [HttpPost]
//    public async Task<ActionResult<InventoryBatchDto>> Create([FromBody] CreateInventoryBatchDto dto)
//    {
//        var created = await _service.CreateAsync(dto);
//        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
//    }

//    [HttpPut("{id}")]
//    public async Task<ActionResult<InventoryBatchDto>> Update(string id, [FromBody] UpdateInventoryBatchDto dto) =>
//        Ok(await _service.UpdateAsync(id, dto));
//}
using AssetHub.Core.Dtos.Inventory;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
  private readonly IInventoryService _service;

  public InventoryController(IInventoryService service)
  {
    _service = service;
  }

  [HttpGet]
  public async Task<ActionResult<List<InventoryBatchDto>>> GetAll([FromQuery] bool includeDeleted = false) =>
      Ok(await _service.GetAllAsync(includeDeleted));

  [HttpGet("{id}")]
  public async Task<ActionResult<InventoryBatchDto>> GetById(string id) => Ok(await _service.GetByIdAsync(id));

  /// <summary>Records one intake. If configuration values are supplied, also bulk-generates the
  /// individual Asset rows for the batch's quantity — see InventoryService.CreateAsync.</summary>
  [HttpPost]
  public async Task<ActionResult<InventoryBatchDto>> Create([FromBody] CreateInventoryBatchDto dto)
  {
    var created = await _service.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<InventoryBatchDto>> Update(string id, [FromBody] UpdateInventoryBatchDto dto) =>
      Ok(await _service.UpdateAsync(id, dto));

  /// <summary>Soft-deletes the batch — it stops appearing in normal listings but the row, its
  /// items, and any assets generated from it are all preserved. Use GET ?includeDeleted=true or
  /// POST {id}/restore to see/undo this.</summary>
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
    await _service.DeleteAsync(id);
    return NoContent();
  }

  [HttpPost("{id}/restore")]
  public async Task<ActionResult<InventoryBatchDto>> Restore(string id) =>
      Ok(await _service.RestoreAsync(id));
}
