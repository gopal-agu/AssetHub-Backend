namespace AssetHub.Core.Entities;

public class Vendor : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Categories this vendor supplies, e.g. ["Laptop","Desktop"]. Stored as JSON.</summary>
    public List<string> Categories { get; set; } = new();

    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string GstNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Rating { get; set; } = 4.0;
    public string Status { get; set; } = "Active";
    public DateOnly OnboardedOn { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<InventoryBatch> InventoryBatches { get; set; } = new List<InventoryBatch>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}
