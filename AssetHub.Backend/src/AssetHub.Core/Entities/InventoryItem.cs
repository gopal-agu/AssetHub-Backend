namespace AssetHub.Core.Entities;

/// <summary>A single child line captured at intake time (payload.items[]) inside an InventoryBatch.</summary>
public class InventoryItem : BaseEntity
{
    public string InventoryBatchId { get; set; } = string.Empty;
    public InventoryBatch? InventoryBatch { get; set; }

    public string Category { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
}
