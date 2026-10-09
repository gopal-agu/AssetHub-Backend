namespace AssetHub.Core.Entities;

public class Employee : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateOnly JoinDate { get; set; }
    public string Status { get; set; } = "Active";
    public string ReportingManager { get; set; } = "—";
    public string Phone { get; set; } = string.Empty;

    public ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();
    public ICollection<AssignmentHistory> AssignmentHistories { get; set; } = new List<AssignmentHistory>();
}
