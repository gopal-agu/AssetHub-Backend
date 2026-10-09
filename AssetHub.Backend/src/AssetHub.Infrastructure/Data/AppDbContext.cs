using AssetHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<InventoryBatch> InventoryBatches => Set<InventoryBatch>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssignmentHistory> AssignmentHistories => Set<AssignmentHistory>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Sequence> Sequences => Set<Sequence>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SoftwareSubscription> SoftwareSubscriptions => Set<SoftwareSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
