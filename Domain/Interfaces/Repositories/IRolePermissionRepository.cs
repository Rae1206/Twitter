using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IRolePermissionRepository : IGenericRepository<RolePermission, Guid>
{
    Task<List<RolePermission>> GetByRoleIdAsync(Guid roleId);
    Task AssignAsync(Guid roleId, Guid permissionId);
    Task RemoveAsync(Guid roleId, Guid permissionId);
}
