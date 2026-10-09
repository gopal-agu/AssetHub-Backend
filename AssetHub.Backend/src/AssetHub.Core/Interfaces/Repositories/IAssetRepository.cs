using AssetHub.Core.Entities;

namespace AssetHub.Core.Interfaces.Repositories;

public interface IAssetRepository : IGenericRepository<Asset>
{
    /// <summary>Assets with Vendor/AssignedEmployee eagerly loaded, for list/detail views.</summary>
    Task<IReadOnlyList<Asset>> GetAllWithRelationsAsync();
    Task<Asset?> GetByIdWithRelationsAsync(string id);
    Task<IReadOnlyList<Asset>> GetByCategoryAsync(string category);
    Task<IReadOnlyList<Asset>> GetByAssignedEmployeeAsync(string employeeId);
}
