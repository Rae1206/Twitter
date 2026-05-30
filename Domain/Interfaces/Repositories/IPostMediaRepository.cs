using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de archivos multimedia adjuntos en publicaciones, heredando de <see cref="IGenericRepository{PostMedia, Guid}"/>.
/// </summary>
public interface IPostMediaRepository : IGenericRepository<PostMedia, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona la lista de archivos multimedia huérfanos (que no están asociados a ningún post) creados antes de la fecha límite especificada.
    /// Útil para tareas de depuración en segundo plano.
    /// </summary>
    /// <param name="cutoff">Fecha límite de corte.</param>
    /// <returns>Una lista conteniendo los archivos multimedia huérfanos <see cref="PostMedia"/>.</returns>
    Task<List<PostMedia>> GetOrphansOlderThanAsync(DateTime cutoff);

    /// <summary>
    /// Obtiene de forma asíncrona la lista de archivos multimedia vinculados a una publicación específica.
    /// </summary>
    /// <param name="postId">Identificador único del post.</param>
    /// <returns>Una lista de recursos multimedia <see cref="PostMedia"/> asociados al post.</returns>
    Task<List<PostMedia>> GetByPostIdAsync(Guid postId);
}
