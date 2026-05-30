using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de permisos de seguridad, heredando de <see cref="IGenericRepository{Permission, Guid}"/>.
/// </summary>
public interface IPermissionRepository : IGenericRepository<Permission, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona todos los permisos asignados a un rol específico del sistema.
    /// </summary>
    /// <param name="roleId">Identificador único del rol.</param>
    /// <returns>Una lista de entidades de permisos <see cref="Permission"/> del rol.</returns>
    Task<List<Permission>> GetPermissionsByRoleAsync(Guid roleId);

    /// <summary>
    /// Obtiene de forma asíncrona todos los permisos agregados de un usuario específico calculados a través de todos sus roles asignados.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Una lista de permisos únicos <see cref="Permission"/> del usuario.</returns>
    Task<List<Permission>> GetPermissionsByUserIdAsync(Guid userId);
}
