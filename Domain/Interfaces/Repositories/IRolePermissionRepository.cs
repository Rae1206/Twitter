using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de relaciones entre roles y permisos, heredando de <see cref="IGenericRepository{RolePermission, Guid}"/>.
/// </summary>
public interface IRolePermissionRepository : IGenericRepository<RolePermission, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona la lista de relaciones de permisos asignados a un rol de seguridad específico.
    /// </summary>
    /// <param name="roleId">Identificador único del rol.</param>
    /// <returns>Una lista conteniendo los registros de vinculación <see cref="RolePermission"/>.</returns>
    Task<List<RolePermission>> GetByRoleIdAsync(Guid roleId);

    /// <summary>
    /// Asigna de forma asíncrona un permiso a un rol de seguridad creando la relación correspondiente.
    /// Valida que la relación no se encuentre asignada previamente para evitar duplicados.
    /// </summary>
    /// <param name="roleId">Identificador único del rol.</param>
    /// <param name="permissionId">Identificador único del permiso.</param>
    Task AssignAsync(Guid roleId, Guid permissionId);

    /// <summary>
    /// Remueve de forma asíncrona la asignación de un permiso sobre un rol de seguridad.
    /// </summary>
    /// <param name="roleId">Identificador único del rol.</param>
    /// <param name="permissionId">Identificador único del permiso.</param>
    Task RemoveAsync(Guid roleId, Guid permissionId);
}
