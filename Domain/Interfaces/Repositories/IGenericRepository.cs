using System.Linq.Expressions;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Generic repository interface for query operations.
/// Write operations are handled by IUnitOfWork.
/// </summary>
public interface IGenericRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id);
    Task<List<T>> GetAllAsync(int limit = 0, int offset = 0, Expression<Func<T, bool>>? filter = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);
    Task<bool> ExistsAsync(TKey id);
    Task<T?> GetByFieldAsync(Expression<Func<T, bool>> expression);
}
