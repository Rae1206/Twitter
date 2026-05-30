using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para la consulta, asociación y purga de los archivos multimedia adjuntos a las publicaciones.
/// </summary>
public class PostMediaRepository : GenericRepository<PostMedia, Guid>, IPostMediaRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PostMediaRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public PostMediaRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista de archivos multimedia huérfanos (que no están asociados a ningún post) creados antes de la fecha límite especificada.
    /// </summary>
    /// <param name="cutoff">Fecha límite de corte.</param>
    /// <returns>Una lista de recursos multimedia huérfanos <see cref="PostMedia"/>.</returns>
    public async Task<List<PostMedia>> GetOrphansOlderThanAsync(DateTime cutoff)
    {
        return await _context.Set<PostMedia>()
            .Where(m => m.PostId == null && m.CreatedAt < cutoff)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista de archivos multimedia vinculados a una publicación específica.
    /// </summary>
    /// <param name="postId">Identificador único de la publicación.</param>
    /// <returns>Una lista de los archivos multimedia <see cref="PostMedia"/> asociados a la publicación.</returns>
    public async Task<List<PostMedia>> GetByPostIdAsync(Guid postId)
    {
        return await _context.Set<PostMedia>()
            .Where(m => m.PostId == postId)
            .ToListAsync();
    }
}
