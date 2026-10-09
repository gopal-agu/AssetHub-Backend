//using AssetHub.Core.Dtos.Asset;
//using AssetHub.Core.Interfaces.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace AssetHub.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[Authorize]
//public class AssignmentHistoryController : ControllerBase
//{
//    private readonly IAssetService _assetService;

//    public AssignmentHistoryController(IAssetService assetService)
//    {
//        _assetService = assetService;
//    }

//    /// <summary>GET /api/assignmenthistory — the full feed the Reports.jsx page reads.</summary>
//    [HttpGet]
//    public async Task<ActionResult<List<AssignmentHistoryDto>>> GetAll() =>
//        Ok(await _assetService.GetAssignmentHistoryAsync());
//}
using AssetHub.Core.Dtos.Asset;
using AssetHub.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetHub.API.Controllers;

[ApiController]
[Route("api/assignmenthistory")]
[Route("api/assignments")]
[Authorize]
public class AssignmentHistoryController : ControllerBase
{
  private readonly IAssetService _assetService;

  public AssignmentHistoryController(IAssetService assetService)
  {
    _assetService = assetService;
  }

  /// <summary>GET /api/assignmenthistory (and /api/assignments, for the frontend's api.assignments.getAll()) —
  /// the full feed the Reports.jsx page reads.</summary>
  [HttpGet]
  public async Task<ActionResult<List<AssignmentHistoryDto>>> GetAll() =>
      Ok(await _assetService.GetAssignmentHistoryAsync());
}
