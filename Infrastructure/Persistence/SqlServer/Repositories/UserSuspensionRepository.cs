using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para la consulta y registro de suspensiones de cuentas de usuarios del sistema.
/// </summary>
public class UserSuspensionRepository : GenericRepository<UserSuspension, Guid>, IUserSuspensionRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UserSuspensionRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public UserSuspensionRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona la última suspensión activa de un usuario determinado, ordenada descendentemente por fecha de creación.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La entidad de suspensión <see cref="UserSuspension"/> activa o null si no posee suspensiones vigentes.</returns>
    public async Task<UserSuspension?> GetActiveSuspensionAsync(Guid userId)
    {
        return await _context.UserSuspensions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
