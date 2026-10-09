using AssetHub.Core.Entities;

namespace AssetHub.Core.Interfaces.Repositories;

/// <summary>
/// Exposes one generic repository per aggregate plus the handful of repositories with custom query
/// needs, and commits everything through a single SaveChangesAsync so a business operation that
/// touches several tables (e.g. assigning an asset + writing an AssignmentHistory row) is atomic.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Vendor> Vendors { get; }
    IGenericRepository<Employee> Employees { get; }
    IGenericRepository<InventoryItem> InventoryItems { get; }
    IGenericRepository<AssignmentHistory> AssignmentHistories { get; }
    IGenericRepository<MaintenanceRecord> MaintenanceRecords { get; }
    IGenericRepository<ActivityLog> ActivityLogs { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<SoftwareSubscription> SoftwareSubscriptions { get; }

    IAssetRepository Assets { get; }
    IInventoryBatchRepository InventoryBatches { get; }

    Task<int> SaveChangesAsync();
}
