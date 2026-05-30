using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para el control y persistencia de las vinculaciones de muchos a muchos entre roles y permisos.
/// </summary>
public class RolePermissionRepository : GenericRepository<RolePermission, Guid>, IRolePermissionRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="RolePermissionRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public RolePermissionRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista de relaciones de vinculación asociadas a un rol de seguridad cargando el detalle de cada permiso.
    /// </summary>
    /// <param name="roleId">Identificador único del rol.</param>
    /// <returns>Una lista conteniendo las entidades de relación <see cref="RolePermission"/>.</returns>
    public async Task<List<RolePermission>> GetByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .ToListAsync();
    }

    /// <summary>
    /// Asigna de forma asíncrona un permiso a un rol de seguridad insertando el registro asociativo correspondiente.
    /// Valida previamente que no exista dicha asignación activa.
    /// </summary>
    /// <param name="roleId">Identificador del rol.</param>
    /// <param name="permissionId">Identificador del permiso.</param>
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

    /// <summary>
    /// Remueve de forma asíncrona la asignación de un permiso sobre un rol en particular eliminando su registro asociativo.
    /// </summary>
    /// <param name="roleId">Identificador del rol.</param>
    /// <param name="permissionId">Identificador del permiso.</param>
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
