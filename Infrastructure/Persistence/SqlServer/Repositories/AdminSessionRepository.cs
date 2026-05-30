using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para el almacenamiento, seguimiento y validación de las sesiones iniciadas por administradores.
/// </summary>
public class AdminSessionRepository : GenericRepository<AdminSession, Guid>, IAdminSessionRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AdminSessionRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public AdminSessionRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona todas las sesiones activas (sin fecha de cierre de sesión) de un administrador, ordenadas descendentemente por fecha de inicio.
    /// </summary>
    /// <param name="adminUserId">Identificador del administrador.</param>
    /// <returns>Una lista de sesiones de administración activas <see cref="AdminSession"/>.</returns>
    public async Task<List<AdminSession>> GetActiveSessionsAsync(Guid adminUserId)
    {
        return await _context.AdminSessions
            .Where(s => s.AdminUserId == adminUserId && s.LogoutAt == null)
            .OrderByDescending(s => s.LoginAt)
            .ToListAsync();
    }

    /// <summary>
    /// Registra de forma asíncrona una nueva sesión de administración en el almacén de datos.
    /// </summary>
    /// <param name="session">La entidad de sesión a persistir.</param>
    public async Task CreateAsync(AdminSession session)
    {
        await _context.AdminSessions.AddAsync(session);
    }
}
