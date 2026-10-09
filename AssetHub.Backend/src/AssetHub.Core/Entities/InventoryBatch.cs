//namespace AssetHub.Core.Entities;

///// <summary>
///// A goods-receipt / intake record (frontend calls this "Inventory item", id format GRN-yyyy-000001).
///// One batch can generate many child InventoryItems and, when driven from a category's config schema,
///// many Asset records that share the batch's vendor/warranty/configuration.
///// </summary>
//public class InventoryBatch : BaseEntity
//{
//    public string Category { get; set; } = string.Empty;
//    public string Department { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;

//    public string? VendorId { get; set; }
//    public Vendor? Vendor { get; set; }

//    public int Quantity { get; set; } = 1;
//    public decimal UnitCost { get; set; }
//    public decimal TotalCost { get; set; }
//    public decimal PurchaseCost { get; set; }

//    public DateOnly PurchaseDate { get; set; }
//    public DateOnly WarrantyStartDate { get; set; }
//    public DateOnly? WarrantyEndDate { get; set; }

//    public string Location { get; set; } = string.Empty;
//    public string Condition { get; set; } = "New";
//    public string Status { get; set; } = "Available";

//    public string InvoiceNo { get; set; } = string.Empty;
//    public string PoNumber { get; set; } = string.Empty;
//    public string Remarks { get; set; } = string.Empty;
//    public string AttachmentName { get; set; } = string.Empty;
//    public string ReceivedBy { get; set; } = "IT Asset Desk";

//    /// <summary>Category-specific configuration captured at intake, e.g. { "cpu": "...", "ram": "16 GB" }.</summary>
//    public Dictionary<string, string> Configuration { get; set; } = new();
//    public string ConfigSummary { get; set; } = string.Empty;

//    public int AssetsGenerated { get; set; }

//    public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
//    public ICollection<Asset> Assets { get; set; } = new List<Asset>();


//}

namespace AssetHub.Core.Entities;

/// <summary>
/// A goods-receipt / intake record (frontend calls this "Inventory item", id format GRN-yyyy-000001).
/// One batch can generate many child InventoryItems and, when driven from a category's config schema,
/// many Asset records that share the batch's vendor/warranty/configuration.
/// </summary>
public class InventoryBatch : BaseEntity
{
  public string Category { get; set; } = string.Empty;
  public string Department { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;

  public string? VendorId { get; set; }
  public Vendor? Vendor { get; set; }

  public int Quantity { get; set; } = 1;
  public decimal UnitCost { get; set; }
  public decimal TotalCost { get; set; }
  public decimal PurchaseCost { get; set; }

  public DateOnly PurchaseDate { get; set; }
  public DateOnly WarrantyStartDate { get; set; }
  public DateOnly? WarrantyEndDate { get; set; }

  public string Location { get; set; } = string.Empty;
  public string Condition { get; set; } = "New";
  public string Status { get; set; } = "Available";

  public string InvoiceNo { get; set; } = string.Empty;
  public string PoNumber { get; set; } = string.Empty;
  public string Remarks { get; set; } = string.Empty;
  public string AttachmentName { get; set; } = string.Empty;
  public string ReceivedBy { get; set; } = "IT Asset Desk";

  /// <summary>Category-specific configuration captured at intake, e.g. { "cpu": "...", "ram": "16 GB" }.</summary>
  public Dictionary<string, string> Configuration { get; set; } = new();
  public string ConfigSummary { get; set; } = string.Empty;

  public int AssetsGenerated { get; set; }

  /// <summary>Soft-delete flag. Deleted batches are excluded from every normal query via the
  /// EF Core query filter in InventoryBatchConfiguration — the row (and its history) is never
  /// physically removed, so assets/audit trail tied to it stay intact.</summary>
  public bool IsDeleted { get; set; }
  public DateTime? DeletedAt { get; set; }

  public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
  public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
