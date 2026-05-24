using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IRoleRepository : IGenericRepository<Role, Guid>
{
    Task<Role?> GetByNameAsync(string name);
    Task<Guid?> GetRoleIdByNameAsync(string roleName);
    Task<List<Role>> GetRolesByUserIdAsync(Guid userId);
    Task<string?> GetPrimaryRoleNameAsync(Guid userId);
}
