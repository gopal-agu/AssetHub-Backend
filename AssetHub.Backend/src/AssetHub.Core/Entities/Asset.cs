namespace AssetHub.Core.Entities;

/// <summary>
/// One physical/logical unit tracked end to end (frontend id format AST-<CATPREFIX>-00047).
/// Generated either individually or in bulk from an InventoryBatch intake.
/// </summary>
public class Asset : BaseEntity
{
    public string SerialNumber { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
    public string? CategoryId { get; set; }

    public Dictionary<string, string> Configuration { get; set; } = new();
    public string ConfigSummary { get; set; } = string.Empty;

    public string? BatchId { get; set; }
    public InventoryBatch? Batch { get; set; }

    public string? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public DateOnly PurchaseDate { get; set; }
    public decimal UnitCost { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public int WarrantyMonths { get; set; }
    public DateOnly? WarrantyExpiry { get; set; }

    public string Location { get; set; } = string.Empty;
    public string Condition { get; set; } = "Good";
    public string Status { get; set; } = "Available";

    public string? AssignedTo { get; set; }
    public Employee? AssignedEmployee { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedDept { get; set; }
    public DateOnly? AssignedOn { get; set; }

    public string Notes { get; set; } = string.Empty;

    public ICollection<AssignmentHistory> AssignmentHistories { get; set; } = new List<AssignmentHistory>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}
