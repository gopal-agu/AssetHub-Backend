namespace AssetHub.Core.Dtos.Dashboard;

public class DashboardSummaryDto
{
    public int TotalAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int UnderRepairAssets { get; set; }
    public int RetiredAssets { get; set; }
    public int DamagedAssets { get; set; }
    public int TotalVendors { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalCategories { get; set; }
    public int OpenMaintenanceTickets { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int AssetsUnderWarranty { get; set; }
    public int AssetsWarrantyExpiringSoon { get; set; }
    public List<CategoryBreakdownDto> AssetsByCategory { get; set; } = new();
    public List<StatusBreakdownDto> AssetsByStatus { get; set; } = new();
    public List<ActivityLogDto> RecentActivity { get; set; } = new();
}

public class CategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class StatusBreakdownDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ActivityLogDto
{
    public int Id { get; set; }
    public string Ts { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class LogActivityRequestDto
{
  public string Text { get; set; } = string.Empty;
}
