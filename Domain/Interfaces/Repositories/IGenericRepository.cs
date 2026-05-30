using System.Linq.Expressions;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz genérica para repositorios de sólo lectura y consultas sobre entidades.
/// Las operaciones de escritura y persistencia son manejadas por el Unit of Work.
/// </summary>
/// <typeparam name="T">El tipo de la entidad administrada por el repositorio.</typeparam>
/// <typeparam name="TKey">El tipo de la clave primaria de la entidad.</typeparam>
public interface IGenericRepository<T, TKey> where T : class
{
    /// <summary>
    /// Obtiene de forma asíncrona una entidad por su identificador único.
    /// </summary>
    /// <param name="id">El identificador único de la entidad.</param>
    /// <returns>La entidad encontrada o null si no existe.</returns>
    Task<T?> GetByIdAsync(TKey id);

    /// <summary>
    /// Obtiene de forma asíncrona un listado paginado y filtrado de todas las entidades.
    /// </summary>
    /// <param name="limit">Cantidad máxima de registros a retornar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="filter">Expresión de filtro condicional opcional.</param>
    /// <returns>Una lista conteniendo las entidades que coinciden con los criterios.</returns>
    Task<List<T>> GetAllAsync(int limit = 0, int offset = 0, Expression<Func<T, bool>>? filter = null);

    /// <summary>
    /// Verifica de forma asíncrona si existe alguna entidad que cumpla con la condición especificada.
    /// </summary>
    /// <param name="expression">La expresión condicional a evaluar.</param>
    /// <returns>True si existe al menos una entidad coincidente; de lo contrario, False.</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// Verifica de forma asíncrona si existe una entidad con el identificador único especificado.
    /// </summary>
    /// <param name="id">El identificador único de la entidad.</param>
    /// <returns>True si la entidad existe; de lo contrario, False.</returns>
    Task<bool> ExistsAsync(TKey id);

    /// <summary>
    /// Obtiene de forma asíncrona la primera entidad que cumpla con la condición especificada.
    /// </summary>
    /// <param name="expression">La expresión condicional a evaluar.</param>
    /// <returns>La entidad encontrada o null si ninguna coincide.</returns>
    Task<T?> GetByFieldAsync(Expression<Func<T, bool>> expression);
}
