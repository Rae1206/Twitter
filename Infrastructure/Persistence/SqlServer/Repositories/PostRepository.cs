using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para la consulta, paginación, filtros de autoría, estados de publicación y limpieza de posts efímeros expirados.
/// </summary>
public class PostRepository : GenericRepository<Post, Guid>, IPostRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PostRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public PostRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene el objeto de consulta IQueryable sobre el conjunto de posts en base de datos.
    /// </summary>
    /// <returns>La consulta IQueryable de tipo <see cref="Post"/>.</returns>
    public IQueryable<Post> GetQueryable()
    {
        return _context.Posts.AsQueryable();
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista paginada de publicaciones filtrando opcionalmente por usuario autor y estado (publicado/borrador).
    /// </summary>
    /// <param name="limit">Cantidad máxima de posts a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="userId">Identificador único del autor opcional.</param>
    /// <param name="isPublished">Filtro opcional de estado de publicación.</param>
    /// <returns>Una lista de posts <see cref="Post"/> que coinciden con los criterios.</returns>
    public async Task<List<Post>> GetAllAsync(int limit, int offset, Guid? userId = null, bool? isPublished = null)
    {
        var query = _context.Posts.AsQueryable();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona todos los posts creados por un usuario específico de forma paginada.
    /// </summary>
    /// <param name="userId">Identificador del usuario autor.</param>
    /// <param name="limit">Cantidad máxima de publicaciones a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista conteniendo los posts del usuario.</returns>
    public async Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int limit = 0, int offset = 0)
    {
        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await _context.Posts
            .Where(p => p.UserId == userId)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona los posts efímeros ya vencidos (con fecha menor o igual al corte cutoff) que aún no hayan sido soft-deleted.
    /// Utiliza IgnoreQueryFilters para bypassear el filtro de consulta global (que ya oculta los posts vencidos del feed).
    /// </summary>
    /// <param name="cutoff">Fecha y hora de corte.</param>
    /// <param name="batchSize">Cantidad máxima de elementos a retornar.</param>
    /// <returns>Una lista de posts vencidos pendientes de marcar como eliminados lógicamente.</returns>
    public async Task<List<Post>> GetExpiredPendingSoftDeleteAsync(DateTime cutoff, int batchSize)
    {
        // IgnoreQueryFilters() hace bypass del filtro global; necesario porque el filtro YA oculta
        // posts vencidos. El cleanup necesita verlos para marcar DeletedAt.
        return await _context.Posts
            .IgnoreQueryFilters()
            .Where(p => p.DeletedAt == null
                        && p.ExpiresAt != null
                        && p.ExpiresAt <= cutoff)
            .OrderBy(p => p.ExpiresAt)
            .Take(batchSize)
            .ToListAsync();
    }
}
