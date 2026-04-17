using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de roles.
/// </summary>
public class RoleRepository : GenericRepository<Role, Guid>, IRoleRepository
{
    public RoleRepository(TwitterDbContext context) : base(context)
    {
    }

    public override Role? GetByField(Expression<Func<Role, bool>> expression) =>
        _dbSet.FirstOrDefault(expression);

    public Role? GetByName(string name) =>
        GetByField(r => r.Name == name);

    public List<Role> GetAll() =>
        base.GetAll(0, 0, r => r.IsActive);

    public Guid? GetRoleIdByName(string roleName)
    {
        return _dbSet
            .Where(r => r.Name == roleName && r.IsActive)
            .Select(r => r.RoleId)
            .FirstOrDefault();
    }

    public List<Role> GetRolesByUserId(Guid userId)
    {
        return _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToList();
    }

    public string? GetPrimaryRoleName(Guid userId)
    {
        var userRole = _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .OrderBy(ur => ur.AssignedAt)
            .FirstOrDefault();
        
        return userRole?.Role?.Name;
    }
}