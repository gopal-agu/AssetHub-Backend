using AssetHub.Core.Dtos.Asset;

namespace AssetHub.Core.Dtos.Inventory;

public class InventoryItemDto
{
    public string Id { get; set; } = string.Empty;
    public string InventoryId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
}

public class InventoryItemInputDto
{
    public string? Category { get; set; }
    public string? Item { get; set; }
    public string? Brand { get; set; }
    public string? Configuration { get; set; }
    public string? SerialNo { get; set; }
}

public class InventoryBatchDto
{
    public string Id { get; set; } = string.Empty;
    public string InventoryId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal PurchaseCost { get; set; }
    public string PurchaseDate { get; set; } = string.Empty;
    public string WarrantyStartDate { get; set; } = string.Empty;
    public string? WarrantyEndDate { get; set; }
    public string? WarrantyExpiry { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public string PoNumber { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string AttachmentName { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public int AssetsGenerated { get; set; }
    public Dictionary<string, string> Configuration { get; set; } = new();
    public string ConfigSummary { get; set; } = string.Empty;
    public List<InventoryItemDto> Items { get; set; } = new();
    public List<AssetDto> Assets { get; set; } = new();
}

/// <summary>
/// Mirrors the payload Inventory.jsx builds when calling addInventoryBatch(): a batch header plus
/// zero or more free-form child items. When Configuration is supplied (category schema fields), the
/// service also auto-generates <see cref="Quantity"/> Asset rows from it — the "avoid manual entry" flow.
/// </summary>
public class CreateInventoryBatchDto
{
    public string? InventoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VendorId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? PurchaseCost { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string PurchaseDate { get; set; } = string.Empty;
    public string? WarrantyStartDate { get; set; }
    public string? WarrantyEndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Condition { get; set; } = "New";
    public string Status { get; set; } = "Available";
    public string InvoiceNo { get; set; } = string.Empty;
    public string PoNumber { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string AttachmentName { get; set; } = string.Empty;
    public Dictionary<string, string>? Configuration { get; set; }
    public List<InventoryItemInputDto> Items { get; set; } = new();
}

public class UpdateInventoryBatchDto
{
    public string? Category { get; set; }
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string? VendorId { get; set; }
    public int? Quantity { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? PurchaseDate { get; set; }
    public string? WarrantyStartDate { get; set; }
    public string? WarrantyEndDate { get; set; }
    public string? Location { get; set; }
    public string? Condition { get; set; }
    public string? Status { get; set; }
    public string? InvoiceNo { get; set; }
    public string? PoNumber { get; set; }
    public string? Remarks { get; set; }
    public string? AttachmentName { get; set; }
}
