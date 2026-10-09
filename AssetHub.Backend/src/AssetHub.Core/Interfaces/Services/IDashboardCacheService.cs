using AssetHub.Core.Dtos.Dashboard;

namespace AssetHub.Core.Interfaces.Services;

/// <summary>
/// High-performance thread-safe in-memory cache for Dashboard summaries.
/// Provides sub-millisecond retrieval and proactive invalidation when entities change.
/// </summary>
public interface IDashboardCacheService
{
    Task<DashboardSummaryDto> GetOrComputeAsync(Func<Task<DashboardSummaryDto>> factory, TimeSpan? expiration = null);
    void Invalidate();
}
