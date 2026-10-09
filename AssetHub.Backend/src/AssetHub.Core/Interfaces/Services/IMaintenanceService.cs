using AssetHub.Core.Dtos.Maintenance;

namespace AssetHub.Core.Interfaces.Services;

public interface IMaintenanceService
{
    Task<List<MaintenanceRecordDto>> GetAllAsync(string? assetId = null, string? status = null);
    Task<MaintenanceRecordDto> GetByIdAsync(string id);
    Task<MaintenanceRecordDto> RaiseAsync(RaiseMaintenanceDto dto);
    Task<MaintenanceRecordDto> CloseAsync(string id, CloseMaintenanceDto dto);
}
