using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IPermissionRepository : IGenericRepository<Permission, Guid>
{
    Task<List<Permission>> GetPermissionsByRoleAsync(Guid roleId);
    Task<List<Permission>> GetPermissionsByUserIdAsync(Guid userId);
}
