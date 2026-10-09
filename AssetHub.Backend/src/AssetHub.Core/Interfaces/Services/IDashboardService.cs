using AssetHub.Core.Dtos.Dashboard;

namespace AssetHub.Core.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
