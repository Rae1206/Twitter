using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de roles - solo lectura.
/// </summary>
public class RoleRepository : GenericRepository<Role, Guid>, IRoleRepository
{
    public RoleRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
        => await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);

    public override async Task<List<Role>> GetAllAsync(int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<Role, bool>>? filter = null)
    {
        var query = _context.Roles.Where(r => r.IsActive);

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }

    public async Task<Guid?> GetRoleIdByNameAsync(string roleName)
        => await _context.Roles
            .Where(r => r.Name == roleName && r.IsActive)
            .Select(r => (Guid?)r.RoleId)
            .FirstOrDefaultAsync();

    public async Task<List<Role>> GetRolesByUserIdAsync(Guid userId)
        => await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync();

    public async Task<string?> GetPrimaryRoleNameAsync(Guid userId)
    {
        var userRole = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .OrderBy(ur => ur.AssignedAt)
            .FirstOrDefaultAsync();
        return userRole?.Role?.Name;
    }
}
