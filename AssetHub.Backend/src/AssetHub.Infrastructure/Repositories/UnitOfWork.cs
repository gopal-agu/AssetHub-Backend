using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Infrastructure.Data;

namespace AssetHub.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Vendor>? _vendors;
    private IGenericRepository<Employee>? _employees;
    private IGenericRepository<InventoryItem>? _inventoryItems;
    private IGenericRepository<AssignmentHistory>? _assignmentHistories;
    private IGenericRepository<MaintenanceRecord>? _maintenanceRecords;
    private IGenericRepository<ActivityLog>? _activityLogs;
    private IGenericRepository<User>? _users;
    private IGenericRepository<SoftwareSubscription>? _softwareSubscriptions;
    private IAssetRepository? _assets;
    private IInventoryBatchRepository? _inventoryBatches;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
    public IGenericRepository<Vendor> Vendors => _vendors ??= new GenericRepository<Vendor>(_context);
    public IGenericRepository<Employee> Employees => _employees ??= new GenericRepository<Employee>(_context);
    public IGenericRepository<InventoryItem> InventoryItems => _inventoryItems ??= new GenericRepository<InventoryItem>(_context);
    public IGenericRepository<AssignmentHistory> AssignmentHistories => _assignmentHistories ??= new GenericRepository<AssignmentHistory>(_context);
    public IGenericRepository<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords ??= new GenericRepository<MaintenanceRecord>(_context);
    public IGenericRepository<ActivityLog> ActivityLogs => _activityLogs ??= new GenericRepository<ActivityLog>(_context);
    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<SoftwareSubscription> SoftwareSubscriptions => _softwareSubscriptions ??= new GenericRepository<SoftwareSubscription>(_context);

    public IAssetRepository Assets => _assets ??= new AssetRepository(_context);
    public IInventoryBatchRepository InventoryBatches => _inventoryBatches ??= new InventoryBatchRepository(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
