namespace AssetHub.Core.Dtos.Employee;

public class EmployeeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string JoinDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReportingManager { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int AssetCount { get; set; }
}

public class CreateEmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ReportingManager { get; set; } = "—";
    public string Phone { get; set; } = string.Empty;
}
