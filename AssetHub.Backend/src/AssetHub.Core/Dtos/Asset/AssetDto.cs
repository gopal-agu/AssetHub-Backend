
namespace AssetHub.Core.Dtos.Asset;

public class AssetDto
{
  public string Id { get; set; } = string.Empty;
  public string SerialNumber { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public Dictionary<string, string> Configuration { get; set; } = new();
  public string ConfigSummary { get; set; } = string.Empty;
  public string? BatchId { get; set; }
  public string? VendorId { get; set; }
  public string VendorName { get; set; } = string.Empty;
  public string PurchaseDate { get; set; } = string.Empty;
  public decimal UnitCost { get; set; }
  public string PoNumber { get; set; } = string.Empty;
  public string InvoiceNo { get; set; } = string.Empty;
  public int WarrantyMonths { get; set; }
  public string? WarrantyExpiry { get; set; }
  public string Location { get; set; } = string.Empty;
  public string Condition { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public string? AssignedTo { get; set; }
  public string? AssignedToName { get; set; }
  public string? AssignedDept { get; set; }
  public string? AssignedOn { get; set; }
  public string Notes { get; set; } = string.Empty;
}

public class CreateAssetDto
{
  public string Category { get; set; } = string.Empty;
  public string SerialNumber { get; set; } = string.Empty;
  public Dictionary<string, string> Configuration { get; set; } = new();
  public string? VendorId { get; set; }
  public string? BatchId { get; set; }
  public string PurchaseDate { get; set; } = string.Empty;
  public decimal UnitCost { get; set; }
  public string PoNumber { get; set; } = string.Empty;
  public string InvoiceNo { get; set; } = string.Empty;
  public int WarrantyMonths { get; set; }
  public string Location { get; set; } = string.Empty;
  public string Condition { get; set; } = "Good";
  public string Status { get; set; } = "Available";
}

public class UpdateAssetDto
{
  public string? SerialNumber { get; set; }
  public Dictionary<string, string>? Configuration { get; set; }
  public string? Location { get; set; }
  public string? Condition { get; set; }
  public string? Notes { get; set; }
  public string? Status { get; set; }
  public string? AssignedTo { get; set; }
  public string? AssignedToName { get; set; }
  public string? AssignedDept { get; set; }
  public string? AssignedOn { get; set; }
}

public class AssignAssetDto
{
  public string EmployeeId { get; set; } = string.Empty;
  public string? EmployeeName { get; set; }
  public string? AssignedTo { get; set; }
  public string? AssignedToName { get; set; }
  public string? AssignedDept { get; set; }
  public string? AssignedOn { get; set; }
  public string? Status { get; set; }
  public string? Remarks { get; set; }
  public string? PerformedBy { get; set; }
}

public class TransferAssetDto
{
  public string NewEmployeeId { get; set; } = string.Empty;
  public string? NewEmployeeName { get; set; }
  public string? AssignedTo { get; set; }
  public string? AssignedToName { get; set; }
  public string? AssignedDept { get; set; }
  public string? AssignedOn { get; set; }
  public string? Status { get; set; }
  public string? Remarks { get; set; }
  public string? PerformedBy { get; set; }
}

public class UnassignAssetDto
{
  public string? Remarks { get; set; }
  public string? PerformedBy { get; set; }
}

public class ChangeAssetStatusDto
{
  public string Status { get; set; } = string.Empty;
  public string? Remarks { get; set; }
  public string? PerformedBy { get; set; }
}

public class RetireAssetDto
{
  public string? Reason { get; set; }
}

public class AssignmentHistoryDto
{
  public string Id { get; set; } = string.Empty;
  public string AssetId { get; set; } = string.Empty;
  public string? EmployeeId { get; set; }
  public string? EmployeeName { get; set; }
  public string? Department { get; set; }
  public string Action { get; set; } = string.Empty;
  public string Date { get; set; } = string.Empty;
  public string Remarks { get; set; } = string.Empty;
  public string PerformedBy { get; set; } = string.Empty;
}


public class EmployeeAssetDto
{
  public string AssetId { get; set; } = string.Empty;
  public string AssetName { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string SerialNumber { get; set; } = string.Empty;
  public string ConfigSummary { get; set; } = string.Empty;
  public Dictionary<string, string>? Configuration { get; set; }
  public string? AssignedOn { get; set; }
  public string Status { get; set; } = string.Empty;
  public string Location { get; set; } = string.Empty;
}

