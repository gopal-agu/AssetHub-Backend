using System.Linq.Expressions;

namespace AssetHub.Core.Interfaces.Repositories;

/// <summary>
/// Generic repository over any BaseEntity-derived type. Keeps simple CRUD/query plumbing out of the
/// services so each service only expresses domain rules. Custom, aggregate-specific queries (joins,
/// eager-loads) live in the small number of dedicated repositories (IAssetRepository, etc.) instead
/// of being bolted onto this interface.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>Escape hatch for queries that need .Include()/.OrderBy() before materializing.</summary>
    IQueryable<T> Query();
}
