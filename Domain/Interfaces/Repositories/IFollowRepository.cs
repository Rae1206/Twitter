using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de relaciones de seguimiento (follows) entre usuarios, heredando de <see cref="IGenericRepository{Follow, Guid}"/>.
/// </summary>
public interface IFollowRepository : IGenericRepository<Follow, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona una relación de seguimiento específica entre un seguidor y un usuario seguido.
    /// </summary>
    /// <param name="followerId">Identificador único del usuario seguidor.</param>
    /// <param name="followingId">Identificador único del usuario seguido.</param>
    /// <returns>La entidad de relación <see cref="Follow"/> o null si no se siguen.</returns>
    Task<Follow?> GetFollow(Guid followerId, Guid followingId);

    /// <summary>
    /// Obtiene de forma asíncrona la lista de usuarios que siguen a un usuario específico (seguidores) de forma paginada.
    /// </summary>
    /// <param name="userId">Identificador único del usuario consultado.</param>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de entidades de usuarios seguidores <see cref="User"/>.</returns>
    Task<List<User>> GetFollowers(Guid userId, int limit = 0, int offset = 0);

    /// <summary>
    /// Obtiene de forma asíncrona la lista de usuarios a los que sigue un usuario específico (seguidos) de forma paginada.
    /// </summary>
    /// <param name="userId">Identificador único del usuario consultado.</param>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de entidades de usuarios seguidos <see cref="User"/>.</returns>
    Task<List<User>> GetFollowing(Guid userId, int limit = 0, int offset = 0);

    /// <summary>
    /// Cuenta de forma asíncrona la cantidad total de seguidores que posee un usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La cantidad total de seguidores.</returns>
    Task<int> GetFollowersCount(Guid userId);

    /// <summary>
    /// Cuenta de forma asíncrona la cantidad total de usuarios a los que sigue un usuario en particular.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La cantidad total de usuarios seguidos.</returns>
    Task<int> GetFollowingCount(Guid userId);

    /// <summary>
    /// Verifica de forma asíncrona si existe una relación activa de seguimiento entre dos usuarios específicos.
    /// </summary>
    /// <param name="followerId">Identificador único del usuario presunto seguidor.</param>
    /// <param name="followingId">Identificador único del usuario presunto seguido.</param>
    /// <returns>True si el primer usuario sigue al segundo; de lo contrario, False.</returns>
    Task<bool> IsFollowing(Guid followerId, Guid followingId);
}