//using AssetHub.Core.Dtos.Asset;
//using AssetHub.Core.Interfaces.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace AssetHub.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[Authorize]
//public class AssetsController : ControllerBase
//{
//  private readonly IAssetService _service;

//  public AssetsController(IAssetService service)
//  {
//    _service = service;
//  }

//  /// <summary>GET /api/assets?category=&amp;status=&amp;location=&amp;department= — filters mirror
//  /// the columns the Assets.jsx grid lets the user filter by.</summary>
//  [HttpGet]
//  public async Task<ActionResult<List<AssetDto>>> GetAll(
//      [FromQuery] string? category, [FromQuery] string? status,
//      [FromQuery] string? location, [FromQuery] string? department) =>
//      Ok(await _service.GetAllAsync(category, status, location, department));

//  [HttpGet("{id}")]
//  public async Task<ActionResult<AssetDto>> GetById(string id) => Ok(await _service.GetByIdAsync(id));

//  [HttpPost]
//  public async Task<ActionResult<AssetDto>> Create([FromBody] CreateAssetDto dto)
//  {
//    var created = await _service.CreateAsync(dto);
//    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
//  }

//  [HttpPut("{id}")]
//  public async Task<ActionResult<AssetDto>> Update(string id, [FromBody] UpdateAssetDto dto) =>
//      Ok(await _service.UpdateAsync(id, dto));

//  [HttpPost("{id}/assign")]
//  public async Task<ActionResult<AssetDto>> Assign(string id, [FromBody] AssignAssetDto dto) =>
//      Ok(await _service.AssignAsync(id, dto));

//  [HttpPost("{id}/unassign")]
//  public async Task<ActionResult<AssetDto>> Unassign(string id, [FromBody] UnassignAssetDto dto) =>
//      Ok(await _service.UnassignAsync(id, dto));

//  [HttpPost("{id}/transfer")]
//  public async Task<ActionResult<AssetDto>> Transfer(string id, [FromBody] TransferAssetDto dto) =>
//      Ok(await _service.TransferAsync(id, dto));

//  [HttpPost("{id}/status")]
//  public async Task<ActionResult<AssetDto>> ChangeStatus(string id, [FromBody] ChangeAssetStatusDto dto) =>
//      Ok(await _service.ChangeStatusAsync(id, dto));

//  [HttpPost("{id}/retire")]
//  public async Task<ActionResult<AssetDto>> Retire(string id, [FromBody] RetireAssetDto dto) =>
//      Ok(await _service.RetireAsync(id, dto));

//  /// <summary>GET /api/assets/{id}/history and GET /api/assets/history for the full audit trail.</summary>
//  [HttpGet("{id}/history")]
//  public async Task<ActionResult<List<AssignmentHistoryDto>>> GetHistory(string id) =>
//      Ok(await _service.GetAssignmentHistoryAsync(id));
//}
using AssetHub.Core.Dtos.Asset;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetsController : ControllerBase
{
  private readonly IAssetService _service;

  public AssetsController(IAssetService service)
  {
    _service = service;
  }

  /// <summary>GET /api/assets?category=&amp;status=&amp;location=&amp;department= — filters mirror
  /// the columns the Assets.jsx grid lets the user filter by.</summary>
  [HttpGet]
  public async Task<ActionResult<List<AssetDto>>> GetAll(
      [FromQuery] string? category, [FromQuery] string? status,
      [FromQuery] string? location, [FromQuery] string? department) =>
      Ok(await _service.GetAllAsync(category, status, location, department));

  [HttpGet("{id}")]
  public async Task<ActionResult<AssetDto>> GetById(string id) => Ok(await _service.GetByIdAsync(id));

  /// <summary>GET /api/assets/employee/{employeeId} — returns assets assigned to an employee by Employee ID (HRMS integration).</summary>
  [AllowAnonymous]
  [HttpGet("employee/{employeeId}")]
  public async Task<ActionResult<List<EmployeeAssetDto>>> GetByEmployeeId(string employeeId) =>
    Ok(await _service.GetByEmployeeIdAsync(employeeId));


  [HttpPost]
  public async Task<ActionResult<AssetDto>> Create([FromBody] CreateAssetDto dto)
  {
    var created = await _service.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<AssetDto>> Update(string id, [FromBody] UpdateAssetDto dto) =>
      Ok(await _service.UpdateAsync(id, dto));

  [HttpPost("{id}/assign")]
  public async Task<ActionResult<AssetDto>> Assign(string id, [FromBody] AssignAssetDto dto) =>
      Ok(await _service.AssignAsync(id, dto));

  [HttpPost("{id}/unassign")]
  public async Task<ActionResult<AssetDto>> Unassign(string id, [FromBody] UnassignAssetDto dto) =>
      Ok(await _service.UnassignAsync(id, dto));

  [HttpPost("{id}/transfer")]
  public async Task<ActionResult<AssetDto>> Transfer(string id, [FromBody] TransferAssetDto dto) =>
      Ok(await _service.TransferAsync(id, dto));

  [HttpPost("{id}/status")]
  public async Task<ActionResult<AssetDto>> ChangeStatus(string id, [FromBody] ChangeAssetStatusDto dto) =>
      Ok(await _service.ChangeStatusAsync(id, dto));

  [HttpPost("{id}/retire")]
  public async Task<ActionResult<AssetDto>> Retire(string id, [FromBody] RetireAssetDto dto) =>
      Ok(await _service.RetireAsync(id, dto));

  /// <summary>GET /api/assets/{id}/history and GET /api/assets/history for the full audit trail.</summary>
  [HttpGet("{id}/history")]
  public async Task<ActionResult<List<AssignmentHistoryDto>>> GetHistory(string id) =>
      Ok(await _service.GetAssignmentHistoryAsync(id));
}
