using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de sesiones administrativas, heredando operaciones genéricas para la entidad <see cref="AdminSession"/>.
/// </summary>
public interface IAdminSessionRepository : IGenericRepository<AdminSession, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona todas las sesiones activas (sin fecha de cierre de sesión) de un administrador específico.
    /// </summary>
    /// <param name="adminUserId">Identificador único del usuario administrador.</param>
    /// <returns>Una lista de sesiones activas <see cref="AdminSession"/>.</returns>
    Task<List<AdminSession>> GetActiveSessionsAsync(Guid adminUserId);

    /// <summary>
    /// Registra de forma asíncrona una nueva sesión de administración en el almacén de datos.
    /// </summary>
    /// <param name="session">La entidad con los detalles de la sesión a crear.</param>
    Task CreateAsync(AdminSession session);
}
