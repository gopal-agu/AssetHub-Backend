namespace AssetHub.Core.Entities;

/// <summary>Audit trail of assign / return / transfer actions on an Asset.</summary>
public class AssignmentHistory : BaseEntity
{
    public string AssetId { get; set; } = string.Empty;
    public Asset? Asset { get; set; }

    public string? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string? EmployeeName { get; set; }
    public string? Department { get; set; }

    /// <summary>Assigned | Returned | Transferred</summary>
    public string Action { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = "IT Asset Desk";
}
