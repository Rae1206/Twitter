using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación genérica del patrón Repository.
/// Provee operaciones CRUD básicas para cualquier entidad.
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
/// <typeparam name="TKey">Tipo de clave primaria</typeparam>
public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class
{
    protected readonly TwitterDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(TwitterDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual T Create(T entity)
    {
        _dbSet.Add(entity);
        return entity;
    }

    public virtual T? GetById(TKey id)
    {
        return _dbSet.Find(id);
    }

    public virtual List<T> GetAll(int limit = 0, int offset = 0, Expression<Func<T, bool>>? filter = null)
    {
        var query = filter is null ? _dbSet : _dbSet.Where(filter);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return query
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();
    }

    public virtual T? Update(TKey id, T entity)
    {
        var existingEntity = _dbSet.Find(id);
        if (existingEntity is null)
            return null;

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        return existingEntity;
    }

    public virtual bool Delete(TKey id)
    {
        var entity = _dbSet.Find(id);
        if (entity is null)
            return false;

        _dbSet.Remove(entity);
        return true;
    }

    public virtual bool Exists(TKey id)
    {
        return _dbSet.Find(id) is not null;
    }

    public async Task<bool> IfExists(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().AnyAsync(expression);
    }

    public virtual T? GetByField(Expression<Func<T, bool>> expression)
    {
        return _dbSet.FirstOrDefault(expression);
    }

    public virtual List<T> GetAllByField(Expression<Func<T, bool>> expression, int limit = 0, int offset = 0)
    {
        var query = _dbSet.Where(expression);
        
        if (offset > 0)
            query = query.Skip(offset);
        
        if (limit > 0)
            query = query.Take(limit);
        
        return query.ToList();
    }
}

/// <summary>
/// Extensiones helpers para Entity Framework.
/// </summary>
public static class EfExtensions
{
    /// <summary>
    /// Incluye relaciones para una query.
    /// </summary>
    public static IQueryable<T> WithIncludes<T>(
        this IQueryable<T> query,
        params Expression<Func<T, object>>[] includes) where T : class
    {
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return query;
    }
}
