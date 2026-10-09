
//using AssetHub.Core.Dtos.Asset;

//namespace AssetHub.Core.Interfaces.Services;

//public interface IAssetService
//{
//  Task<List<AssetDto>> GetAllAsync(string? category = null, string? status = null, string? location = null, string? department = null);
//  Task<AssetDto> GetByIdAsync(string id);
//  Task<AssetDto> CreateAsync(CreateAssetDto dto);
//  Task<AssetDto> UpdateAsync(string id, UpdateAssetDto dto);

//  Task<AssetDto> AssignAsync(string assetId, AssignAssetDto dto);
//  Task<AssetDto> UnassignAsync(string assetId, UnassignAssetDto dto);
//  Task<AssetDto> TransferAsync(string assetId, TransferAssetDto dto);
//  Task<AssetDto> ChangeStatusAsync(string assetId, ChangeAssetStatusDto dto);
//  Task<AssetDto> RetireAsync(string assetId, RetireAssetDto dto);

//  Task<List<AssignmentHistoryDto>> GetAssignmentHistoryAsync(string? assetId = null);
//}
using AssetHub.Core.Dtos.Asset;

namespace AssetHub.Core.Interfaces.Services;

public interface IAssetService
{
  Task<List<AssetDto>> GetAllAsync(string? category = null, string? status = null, string? location = null, string? department = null);
  Task<AssetDto> GetByIdAsync(string id);
  Task<List<EmployeeAssetDto>> GetByEmployeeIdAsync(string employeeId);

  Task<AssetDto> CreateAsync(CreateAssetDto dto);
  Task<AssetDto> UpdateAsync(string id, UpdateAssetDto dto);

  Task<AssetDto> AssignAsync(string assetId, AssignAssetDto dto);
  Task<AssetDto> UnassignAsync(string assetId, UnassignAssetDto dto);
  Task<AssetDto> TransferAsync(string assetId, TransferAssetDto dto);
  Task<AssetDto> ChangeStatusAsync(string assetId, ChangeAssetStatusDto dto);
  Task<AssetDto> RetireAsync(string assetId, RetireAssetDto dto);

  Task<List<AssignmentHistoryDto>> GetAssignmentHistoryAsync(string? assetId = null);
}
