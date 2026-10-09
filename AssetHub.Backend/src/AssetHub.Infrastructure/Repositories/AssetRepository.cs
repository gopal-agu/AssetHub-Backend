using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Repositories;

public class AssetRepository : GenericRepository<Asset>, IAssetRepository
{
    public AssetRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Asset>> GetAllWithRelationsAsync() =>
        await DbSet.AsNoTracking()
            .Include(a => a.Vendor)
            .Include(a => a.AssignedEmployee)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<Asset?> GetByIdWithRelationsAsync(string id) =>
        await DbSet
            .Include(a => a.Vendor)
            .Include(a => a.AssignedEmployee)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IReadOnlyList<Asset>> GetByCategoryAsync(string category) =>
        await DbSet.AsNoTracking().Where(a => a.Category == category).ToListAsync();

    public async Task<IReadOnlyList<Asset>> GetByAssignedEmployeeAsync(string employeeId) =>
        await DbSet.AsNoTracking().Where(a => a.AssignedTo == employeeId).ToListAsync();
}
