using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de posts (publicaciones), heredando de <see cref="IGenericRepository{Post, Guid}"/>.
/// </summary>
public interface IPostRepository : IGenericRepository<Post, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona la lista paginada de publicaciones filtrando opcionalmente por usuario creador y estado de publicación.
    /// </summary>
    /// <param name="limit">Cantidad máxima de posts a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="userId">Identificador único del autor opcional para filtrar.</param>
    /// <param name="isPublished">Filtro opcional para obtener borradores o publicados.</param>
    /// <returns>Una lista conteniendo los posts <see cref="Post"/>.</returns>
    Task<List<Post>> GetAllAsync(int limit, int offset, Guid? userId = null, bool? isPublished = null);

    /// <summary>
    /// Obtiene de forma asíncrona todos los posts creados por un usuario específico, con paginación.
    /// </summary>
    /// <param name="userId">Identificador único del autor.</param>
    /// <param name="limit">Cantidad máxima de registros a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de posts del usuario.</returns>
    Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int limit = 0, int offset = 0);

    /// <summary>
    /// Obtiene el objeto de consulta IQueryable para construir consultas personalizadas y optimizadas de posts.
    /// </summary>
    /// <returns>Un IQueryable de tipo <see cref="Post"/>.</returns>
    IQueryable<Post> GetQueryable();

    /// <summary>
    /// Obtiene de forma asíncrona posts efímeros ya expirados (ExpiresAt menor o igual a cutoff) que todavía no han sido eliminados de forma lógica (soft-deleted).
    /// Utiliza IgnoreQueryFilters para saltar el filtro de consulta global que oculta posts vencidos del feed principal.
    /// </summary>
    /// <param name="cutoff">Fecha y hora de corte para la expiración.</param>
    /// <param name="batchSize">Cantidad máxima de registros a procesar en lote.</param>
    /// <returns>Una lista conteniendo los posts efímeros expirados listos para eliminación lógica.</returns>
    Task<List<Post>> GetExpiredPendingSoftDeleteAsync(DateTime cutoff, int batchSize);
}
