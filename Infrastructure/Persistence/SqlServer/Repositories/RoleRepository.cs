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

    public Role? GetByName(string name) => _context.Roles.FirstOrDefault(r => r.Name == name);

    public override List<Role> GetAll(int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<Role, bool>>? filter = null)
    {
        var query = _context.Roles.Where(r => r.IsActive);

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return query
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();
    }

    public Guid? GetRoleIdByName(string roleName) => _context.Roles
        .Where(r => r.Name == roleName && r.IsActive)
        .Select(r => r.RoleId)
        .FirstOrDefault();

    public List<Role> GetRolesByUserId(Guid userId) => _context.UserRoles
        .Where(ur => ur.UserId == userId)
        .Include(ur => ur.Role)
        .Select(ur => ur.Role)
        .ToList();

    public string? GetPrimaryRoleName(Guid userId) => _context.UserRoles
        .Where(ur => ur.UserId == userId)
        .Include(ur => ur.Role)
        .OrderBy(ur => ur.AssignedAt)
        .FirstOrDefault()?.Role?.Name;
}
