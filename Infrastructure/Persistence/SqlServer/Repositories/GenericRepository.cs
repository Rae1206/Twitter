using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio genérico de consulta de base de datos implementado con Entity Framework Core.
/// Las operaciones de escritura pasan por el Unit of Work.
/// </summary>
/// <typeparam name="T">El tipo de la entidad administrada por el repositorio.</typeparam>
/// <typeparam name="TKey">El tipo de la clave primaria de la entidad.</typeparam>
public class GenericRepository<T, TKey>(TwitterDbContext context) : IGenericRepository<T, TKey> where T : class
{
    /// <summary>
    /// Instancia compartida del contexto de base de datos <see cref="TwitterDbContext"/>.
    /// </summary>
    protected readonly TwitterDbContext _context = context;

    /// <summary>
    /// Obtiene de forma asíncrona una entidad de base de datos por su identificador único.
    /// </summary>
    /// <param name="id">El identificador de la entidad.</param>
    /// <returns>La entidad encontrada o null si no existe.</returns>
    public virtual async Task<T?> GetByIdAsync(TKey id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Obtiene de forma asíncrona una lista paginada de entidades, aplicando opcionalmente un filtro.
    /// </summary>
    /// <param name="limit">Cantidad máxima de registros a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="filter">Filtro de consulta opcional.</param>
    /// <returns>Una lista de entidades que coinciden con los criterios de búsqueda.</returns>
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

    /// <summary>
    /// Verifica de forma asíncrona si existe una entidad con el identificador único especificado.
    /// </summary>
    /// <param name="id">El identificador único de la entidad.</param>
    /// <returns>True si la entidad existe; de lo contrario, False.</returns>
    public virtual async Task<bool> ExistsAsync(TKey id)
    {
        return await _context.Set<T>().FindAsync(id) is not null;
    }

    /// <summary>
    /// Obtiene de forma asíncrona la primera entidad que cumpla con la condición especificada.
    /// </summary>
    /// <param name="expression">La expresión condicional a evaluar.</param>
    /// <returns>La entidad encontrada o null si ninguna coincide.</returns>
    public virtual async Task<T?> GetByFieldAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(expression);
    }

    /// <summary>
    /// Verifica de forma asíncrona si existe alguna entidad que cumpla con la condición especificada.
    /// </summary>
    /// <param name="expression">La expresión condicional a evaluar.</param>
    /// <returns>True si existe al menos una entidad coincidente; de lo contrario, False.</returns>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().AnyAsync(expression);
    }
}
