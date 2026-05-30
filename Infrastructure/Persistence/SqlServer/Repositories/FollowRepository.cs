using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para el almacenamiento y consulta de las relaciones de seguimiento (follows) entre usuarios.
/// </summary>
public class FollowRepository : GenericRepository<Follow, Guid>, IFollowRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="FollowRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public FollowRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona una relación de seguimiento específica entre un seguidor y un usuario seguido.
    /// </summary>
    /// <param name="followerId">Identificador único del usuario seguidor.</param>
    /// <param name="followingId">Identificador único del usuario seguido.</param>
    /// <returns>La entidad de relación <see cref="Follow"/> o null si no se siguen.</returns>
    public async Task<Follow?> GetFollow(Guid followerId, Guid followingId)
    {
        return await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista de usuarios que siguen a un usuario específico (seguidores) de forma paginada.
    /// </summary>
    /// <param name="userId">Identificador del usuario seguido.</param>
    /// <param name="limit">Cantidad máxima de seguidores a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de usuarios seguidores <see cref="User"/>.</returns>
    public async Task<List<User>> GetFollowers(Guid userId, int limit = 0, int offset = 0)
    {
        var query = _context.Follows
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista de usuarios a los que sigue un usuario específico (seguidos) de forma paginada.
    /// </summary>
    /// <param name="userId">Identificador del usuario seguidor.</param>
    /// <param name="limit">Cantidad máxima de usuarios seguidos a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de usuarios seguidos <see cref="User"/>.</returns>
    public async Task<List<User>> GetFollowing(Guid userId, int limit = 0, int offset = 0)
    {
        var query = _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    /// <summary>
    /// Cuenta de forma asíncrona la cantidad total de seguidores que posee un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>La cantidad total de seguidores.</returns>
    public async Task<int> GetFollowersCount(Guid userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowingId == userId);
    }

    /// <summary>
    /// Cuenta de forma asíncrona la cantidad total de usuarios a los que sigue un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>La cantidad total de usuarios seguidos.</returns>
    public async Task<int> GetFollowingCount(Guid userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowerId == userId);
    }

    /// <summary>
    /// Verifica de forma asíncrona si existe una relación activa de seguimiento entre dos usuarios específicos.
    /// </summary>
    /// <param name="followerId">Identificador del seguidor.</param>
    /// <param name="followingId">Identificador del seguido.</param>
    /// <returns>True si existe la relación; de lo contrario, False.</returns>
    public async Task<bool> IsFollowing(Guid followerId, Guid followingId)
    {
        return await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }
}