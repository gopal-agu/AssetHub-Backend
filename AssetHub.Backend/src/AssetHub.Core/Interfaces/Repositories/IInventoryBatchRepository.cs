using AssetHub.Core.Entities;

namespace AssetHub.Core.Interfaces.Repositories;

public interface IInventoryBatchRepository : IGenericRepository<InventoryBatch>
{
    Task<IReadOnlyList<InventoryBatch>> GetAllWithItemsAsync();
    Task<InventoryBatch?> GetByIdWithItemsAsync(string id);
}
