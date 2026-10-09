using AssetHub.Core.Dtos.Dashboard;
using AssetHub.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AssetHub.Infrastructure.Services;

/// <summary>
/// Thread-safe in-memory cache for dashboard summary data with stampede protection (SemaphoreSlim)
/// and proactive invalidation on write events.
/// </summary>
public class DashboardCacheService : IDashboardCacheService
{
    private const string CacheKey = "AssetHub_Dashboard_Summary_Cache";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromSeconds(60);

    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DashboardCacheService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public DashboardCacheService(IMemoryCache memoryCache, ILogger<DashboardCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetOrComputeAsync(Func<Task<DashboardSummaryDto>> factory, TimeSpan? expiration = null)
    {
        // Fast path: cache hit (< 1 ms, typically ~0.001 ms)
        if (_memoryCache.TryGetValue(CacheKey, out DashboardSummaryDto? cached) && cached != null)
        {
            return cached;
        }

        // Cache miss: serialize entry using semaphore to prevent cache stampede
        await _semaphore.WaitAsync();
        try
        {
            // Double-check locking pattern
            if (_memoryCache.TryGetValue(CacheKey, out cached) && cached != null)
            {
                return cached;
            }

            _logger.LogInformation("Dashboard cache miss or expired. Computing fresh summary metrics from database.");
            var summary = await factory();

            var ttl = expiration ?? DefaultExpiration;
            _memoryCache.Set(CacheKey, summary, ttl);

            return summary;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Invalidate()
    {
        _memoryCache.Remove(CacheKey);
        _logger.LogInformation("Dashboard cache invalidated due to entity modification.");
    }
}
