using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RolePermissionRepository : GenericRepository<RolePermission, Guid>, IRolePermissionRepository
{
    public RolePermissionRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<List<RolePermission>> GetByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task AssignAsync(Guid roleId, Guid permissionId)
    {
        var exists = await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        if (!exists)
        {
            var rolePermission = new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                AssignedAt = DateTime.UtcNow
            };
            await _context.RolePermissions.AddAsync(rolePermission);
        }
    }

    public async Task RemoveAsync(Guid roleId, Guid permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission is not null)
        {
            _context.RolePermissions.Remove(rolePermission);
        }
    }
}
