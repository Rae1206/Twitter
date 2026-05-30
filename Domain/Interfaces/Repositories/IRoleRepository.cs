using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de roles de seguridad del sistema, heredando de <see cref="IGenericRepository{Role, Guid}"/>.
/// </summary>
public interface IRoleRepository : IGenericRepository<Role, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona un rol mediante su nombre identificativo único.
    /// </summary>
    /// <param name="name">Nombre único del rol.</param>
    /// <returns>La entidad de rol <see cref="Role"/> o null si no existe.</returns>
    Task<Role?> GetByNameAsync(string name);

    /// <summary>
    /// Obtiene de forma asíncrona el identificador único (Guid) de un rol por su nombre, validando que se encuentre activo.
    /// </summary>
    /// <param name="roleName">Nombre identificativo del rol.</param>
    /// <returns>El identificador Guid del rol o null si no se encuentra.</returns>
    Task<Guid?> GetRoleIdByNameAsync(string roleName);

    /// <summary>
    /// Obtiene de forma asíncrona la lista de todos los roles asignados a un usuario específico.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Una lista de roles <see cref="Role"/> asignados al usuario.</returns>
    Task<List<Role>> GetRolesByUserIdAsync(Guid userId);

    /// <summary>
    /// Obtiene de forma asíncrona el nombre del rol primario (el asignado de forma más antigua) de un usuario en particular.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>El nombre del rol primario o null si no posee roles asignados.</returns>
    Task<string?> GetPrimaryRoleNameAsync(Guid userId);
}
