namespace AssetHub.Core.Dtos.Maintenance;

public class MaintenanceRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string OpenedOn { get; set; } = string.Empty;
    public string? ExpectedCompletion { get; set; }
    public string? CompletedOn { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? VendorId { get; set; }
    public string? VendorName { get; set; }
}

public class RaiseMaintenanceDto
{
    public string AssetId { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReportedBy { get; set; }
}

public class CloseMaintenanceDto
{
    public decimal? Cost { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? RestoreStatus { get; set; }
}
