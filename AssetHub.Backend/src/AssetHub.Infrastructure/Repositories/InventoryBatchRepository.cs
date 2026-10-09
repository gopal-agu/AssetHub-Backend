using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Infrastructure.Repositories;

public class InventoryBatchRepository : GenericRepository<InventoryBatch>, IInventoryBatchRepository
{
    public InventoryBatchRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InventoryBatch>> GetAllWithItemsAsync() =>
        await DbSet.AsNoTracking()
            .Include(b => b.Items)
            .Include(b => b.Vendor)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<InventoryBatch?> GetByIdWithItemsAsync(string id) =>
        await DbSet
            .Include(b => b.Items)
            .Include(b => b.Vendor)
            .FirstOrDefaultAsync(b => b.Id == id);
}
