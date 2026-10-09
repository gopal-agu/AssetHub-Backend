using AssetHub.Core.Common;
using AssetHub.Core.Dtos.Dashboard;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Services;

/// <summary>
/// Optimized DashboardService providing sub-millisecond responses via IDashboardCacheService
/// and efficient database-level push-down aggregations on cache miss.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;
    private readonly IActivityLogService _activity;
    private readonly IDashboardCacheService _cache;

    public DashboardService(IUnitOfWork uow, IActivityLogService activity, IDashboardCacheService cache)
    {
        _uow = uow;
        _activity = activity;
        _cache = cache;
    }

    public Task<DashboardSummaryDto> GetSummaryAsync()
    {
        return _cache.GetOrComputeAsync(ComputeSummaryAsync);
    }

    private async Task<DashboardSummaryDto> ComputeSummaryAsync()
    {
        // 1. Group-by Status: returns only ~5 rows of (Status, Count)
        var statusBreakdown = await _uow.Assets.Query()
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new StatusBreakdownDto { Status = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        var statusMap = statusBreakdown.ToDictionary(s => s.Status, s => s.Count, StringComparer.OrdinalIgnoreCase);

        // 2. Group-by Category: returns only category counts (~5-10 rows)
        var categoryBreakdown = await _uow.Assets.Query()
            .AsNoTracking()
            .GroupBy(a => a.Category)
            .Select(g => new CategoryBreakdownDto { Category = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        // 3. Inventory financial & warranty metrics (push-down SQL queries)
        var totalInventoryValue = await _uow.Assets.Query()
            .AsNoTracking()
            .SumAsync(a => (decimal?)a.UnitCost) ?? 0m;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soon = today.AddDays(60);

        var assetsUnderWarranty = await _uow.Assets.Query()
            .AsNoTracking()
            .CountAsync(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value >= today);

        var assetsWarrantyExpiringSoon = await _uow.Assets.Query()
            .AsNoTracking()
            .CountAsync(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value >= today && a.WarrantyExpiry.Value <= soon);

        // 4. Fast scalar counts for other aggregates (returns single 4-byte integers)
        var totalVendors = await _uow.Vendors.CountAsync();
        var totalEmployees = await _uow.Employees.CountAsync();
        var totalCategories = await _uow.Categories.CountAsync();
        var openTickets = await _uow.MaintenanceRecords.CountAsync(m => m.Status != MaintenanceStatuses.Closed);

        // 5. Recent Activity (direct projection of top 40 records)
        var recentActivity = await _activity.GetRecentAsync(40);

        return new DashboardSummaryDto
        {
            TotalAssets = statusBreakdown.Sum(s => s.Count),
            AvailableAssets = statusMap.GetValueOrDefault(AssetStatuses.Available, 0),
            AssignedAssets = statusMap.GetValueOrDefault(AssetStatuses.Assigned, 0),
            UnderRepairAssets = statusMap.GetValueOrDefault(AssetStatuses.UnderRepair, 0),
            RetiredAssets = statusMap.GetValueOrDefault(AssetStatuses.Retired, 0),
            DamagedAssets = statusMap.GetValueOrDefault(AssetStatuses.Damaged, 0),
            TotalVendors = totalVendors,
            TotalEmployees = totalEmployees,
            TotalCategories = totalCategories,
            OpenMaintenanceTickets = openTickets,
            TotalInventoryValue = totalInventoryValue,
            AssetsUnderWarranty = assetsUnderWarranty,
            AssetsWarrantyExpiringSoon = assetsWarrantyExpiringSoon,
            AssetsByCategory = categoryBreakdown,
            AssetsByStatus = statusBreakdown,
            RecentActivity = recentActivity,
        };
    }
}
