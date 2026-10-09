using AssetHub.Core.Dtos.Dashboard;

namespace AssetHub.Core.Interfaces.Services;

public interface IActivityLogService
{
    Task LogAsync(string text);
    Task<List<ActivityLogDto>> GetRecentAsync(int take = 40);
}
