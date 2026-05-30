using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para la consulta y mapeo de permisos asociados a roles y usuarios individuales para el control de accesos.
/// </summary>
public class PermissionRepository : GenericRepository<Permission, Guid>, IPermissionRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PermissionRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public PermissionRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona todos los permisos individuales vinculados a un rol del sistema.
    /// </summary>
    /// <param name="roleId">Identificador único del rol.</param>
    /// <returns>Una lista conteniendo los permisos <see cref="Permission"/> del rol.</returns>
    public async Task<List<Permission>> GetPermissionsByRoleAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona el conjunto consolidado y único de permisos asociados a un usuario consolidando la unión de todos sus roles asignados.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Una lista de permisos únicos <see cref="Permission"/> asignados al usuario.</returns>
    public async Task<List<Permission>> GetPermissionsByUserIdAsync(Guid userId)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        return await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }
}
