//using AssetHub.Core.Dtos.Inventory;

//namespace AssetHub.Core.Interfaces.Services;

//public interface IInventoryService
//{
//    Task<List<InventoryBatchDto>> GetAllAsync();
//    Task<InventoryBatchDto> GetByIdAsync(string id);

//    /// <summary>
//    /// Creates the batch header + child items. When dto.Configuration is supplied (category schema
//    /// values), also bulk-generates dto.Quantity Asset records tagged/serialled off that batch —
//    /// mirrors mockData.js's BATCH_PLAN auto-generation, wrapped in one DB transaction.
//    /// </summary>
//    Task<InventoryBatchDto> CreateAsync(CreateInventoryBatchDto dto);
//    Task<InventoryBatchDto> UpdateAsync(string id, UpdateInventoryBatchDto dto);
//}
using AssetHub.Core.Dtos.Inventory;

namespace AssetHub.Core.Interfaces.Services;

public interface IInventoryService
{
  /// <param name="includeDeleted">When true, also returns soft-deleted batches (for an audit/trash view).</param>
  Task<List<InventoryBatchDto>> GetAllAsync(bool includeDeleted = false);
  Task<InventoryBatchDto> GetByIdAsync(string id);

  /// <summary>
  /// Creates the batch header + child items. When dto.Configuration is supplied (category schema
  /// values), also bulk-generates dto.Quantity Asset records tagged/serialled off that batch —
  /// mirrors mockData.js's BATCH_PLAN auto-generation, wrapped in one DB transaction.
  /// </summary>
  Task<InventoryBatchDto> CreateAsync(CreateInventoryBatchDto dto);
  Task<InventoryBatchDto> UpdateAsync(string id, UpdateInventoryBatchDto dto);

  /// <summary>Soft-deletes the batch (sets IsDeleted/DeletedAt) — the row, its items, and any
  /// assets generated from it are all kept; the batch just stops appearing in normal listings.</summary>
  Task DeleteAsync(string id);

  /// <summary>Un-deletes a previously soft-deleted batch.</summary>
  Task<InventoryBatchDto> RestoreAsync(string id);
}
