using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de likes (me gusta) en publicaciones, heredando de <see cref="IGenericRepository{Like, Guid}"/>.
/// </summary>
public interface ILikeRepository : IGenericRepository<Like, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona un me gusta (like) específico realizado por un usuario a una publicación.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="postId">Identificador único de la publicación.</param>
    /// <returns>La entidad de relación <see cref="Like"/> o null si no existe.</returns>
    Task<Like?> GetLike(Guid userId, Guid postId);

    /// <summary>
    /// Obtiene de forma asíncrona la lista de usuarios que le han dado me gusta a una publicación específica (likers) de forma paginada.
    /// </summary>
    /// <param name="postId">Identificador único de la publicación.</param>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de entidades de usuarios <see cref="User"/>.</returns>
    Task<List<User>> GetLikers(Guid postId, int limit = 0, int offset = 0);
}
