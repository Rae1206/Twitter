using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic query repository. Write operations go through UnitOfWork.
/// </summary>
public class GenericRepository<T, TKey>(TwitterDbContext context) : IGenericRepository<T, TKey> where T : class
{
    protected readonly TwitterDbContext _context = context;

    public virtual async Task<T?> GetByIdAsync(TKey id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync(int limit = 0, int offset = 0, Expression<Func<T, bool>>? filter = null)
    {
        var query = filter is null ? _context.Set<T>() : _context.Set<T>().Where(filter);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? 100 : Math.Min(limit, 100);

        return await query
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }

    public virtual async Task<bool> ExistsAsync(TKey id)
    {
        return await _context.Set<T>().FindAsync(id) is not null;
    }

    public virtual async Task<T?> GetByFieldAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(expression);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().AnyAsync(expression);
    }
}
