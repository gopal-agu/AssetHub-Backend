namespace AssetHub.Core.Entities;

/// <summary>A repair/maintenance ticket (id format MT-yyyy-00001) raised against an Asset.</summary>
public class MaintenanceRecord : BaseEntity
{
    public string AssetId { get; set; } = string.Empty;
    public Asset? Asset { get; set; }

    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;

    public DateOnly OpenedOn { get; set; }
    public DateOnly? ExpectedCompletion { get; set; }
    public DateOnly? CompletedOn { get; set; }

    public string Status { get; set; } = "Open";
    public decimal Cost { get; set; }
    public string? ResolutionNotes { get; set; }

    public string? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
}
