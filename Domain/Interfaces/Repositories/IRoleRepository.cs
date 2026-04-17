using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz para el repositorio de roles.
/// </summary>
public interface IRoleRepository : IGenericRepository<Role, Guid>
{
    Role? GetByName(string name);
    Guid? GetRoleIdByName(string roleName);
    List<Role> GetRolesByUserId(Guid userId);
    string? GetPrimaryRoleName(Guid userId);
}
