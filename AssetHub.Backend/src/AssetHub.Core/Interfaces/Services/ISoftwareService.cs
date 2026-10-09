using AssetHub.Core.Dtos.Software;

namespace AssetHub.Core.Interfaces.Services;

public interface ISoftwareService
{
    Task<List<SoftwareSubscriptionDto>> GetAllAsync(string? category = null, string? status = null);
    Task<SoftwareSubscriptionDto> GetByIdAsync(string id);
    Task<SoftwareSubscriptionDto> CreateAsync(CreateSoftwareSubscriptionDto dto);
    Task<SoftwareSubscriptionDto> UpdateAsync(string id, UpdateSoftwareSubscriptionDto dto);
    Task DeleteAsync(string id);
}
