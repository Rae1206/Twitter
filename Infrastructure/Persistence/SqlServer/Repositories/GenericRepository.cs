using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio genérico de CONSULTA.
/// Usa DbContext internamente, no expone DbSet.
/// Las operaciones de escritura van en UnitOfWork.
/// </summary>
public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class
{
    protected readonly TwitterDbContext _context;

    public GenericRepository(TwitterDbContext context)
    {
        _context = context;
    }

    public virtual T? GetById(TKey id)
    {
        return _context.Set<T>().Find(id);
    }

    public virtual List<T> GetAll(int limit = 0, int offset = 0, Expression<Func<T, bool>>? filter = null)
    {
        var query = filter is null ? _context.Set<T>() : _context.Set<T>().Where(filter);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return query
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();
    }

    public virtual bool Exists(TKey id)
    {
        return _context.Set<T>().Find(id) is not null;
    }

    public virtual T? GetByField(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().FirstOrDefault(expression);
    }

    public async Task<bool> IfExists(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().AnyAsync(expression);
    }
}