using AssetHub.Core.Dtos.Dashboard;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IUnitOfWork _uow;
    private readonly IDashboardCacheService _cache;

    public ActivityLogService(IUnitOfWork uow, IDashboardCacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task LogAsync(string text)
    {
        await _uow.ActivityLogs.AddAsync(new ActivityLog { Text = text, Ts = DateTime.UtcNow });
        await _uow.SaveChangesAsync();
        _cache.Invalidate();
    }

    public async Task<List<ActivityLogDto>> GetRecentAsync(int take = 40)
    {
        var logs = await _uow.ActivityLogs.Query()
            .AsNoTracking()
            .OrderByDescending(a => a.Ts)
            .Take(take)
            .Select(l => new { l.Id, l.Ts, l.Text })
            .ToListAsync();

        return logs.Select(l => new ActivityLogDto
        {
            Id = l.Id,
            Ts = l.Ts.ToString("O"),
            Text = l.Text
        }).ToList();
    }
}
