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
/// Repositorio de base de datos encargado del almacenamiento, control y consulta de las interacciones de 'Me gusta' (likes) en las publicaciones.
/// </summary>
public class LikeRepository : GenericRepository<Like, Guid>, ILikeRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="LikeRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public LikeRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona un registro de 'Me gusta' particular realizado por un usuario a una publicación específica.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="postId">Identificador del post.</param>
    /// <returns>La entidad de interacción <see cref="Like"/> o null si no existe.</returns>
    public async Task<Like?> GetLike(Guid userId, Guid postId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista paginada de usuarios que reaccionaron con 'Me gusta' a una publicación.
    /// </summary>
    /// <param name="postId">Identificador del post.</param>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de usuarios <see cref="User"/>.</returns>
    public async Task<List<User>> GetLikers(Guid postId, int limit = 0, int offset = 0)
    {
        var query = _context.Likes
            .Where(l => l.PostId == postId)
            .Select(l => l.User);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }
}
