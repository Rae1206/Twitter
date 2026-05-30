using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos encargado del almacenamiento, asignaciones dinámicas y consulta de los roles de seguridad de los usuarios.
/// </summary>
public class RoleRepository : GenericRepository<Role, Guid>, IRoleRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="RoleRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public RoleRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona un rol de seguridad buscando por su nombre único.
    /// </summary>
    /// <param name="name">El nombre del rol.</param>
    /// <returns>La entidad <see cref="Role"/> encontrada o null si no existe.</returns>
    public async Task<Role?> GetByNameAsync(string name)
        => await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);

    /// <summary>
    /// Obtiene de forma asíncrona una lista paginada de todos los roles activos del sistema, aplicando opcionalmente un filtro condicional.
    /// </summary>
    /// <param name="limit">Cantidad máxima de roles a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="filter">Filtro de consulta opcional.</param>
    /// <returns>Una lista conteniendo los roles activos <see cref="Role"/>.</returns>
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

    /// <summary>
    /// Obtiene de forma asíncrona el identificador único (Guid) de un rol buscando por su nombre y validando que esté activo.
    /// </summary>
    /// <param name="roleName">Nombre identificativo del rol.</param>
    /// <returns>El identificador Guid del rol o null si no se encuentra registrado y activo.</returns>
    public async Task<Guid?> GetRoleIdByNameAsync(string roleName)
        => await _context.Roles
            .Where(r => r.Name == roleName && r.IsActive)
            .Select(r => (Guid?)r.RoleId)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Obtiene de forma asíncrona la lista de roles activos asignados a un usuario específico a través de la tabla relacional UserRoles.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Una lista conteniendo los roles <see cref="Role"/> del usuario.</returns>
    public async Task<List<Role>> GetRolesByUserIdAsync(Guid userId)
        => await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync();

    /// <summary>
    /// Obtiene de forma asíncrona el nombre del rol primario (el primero en asignársele en orden temporal) de un usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>El nombre del rol primario o null si no tiene asignaciones.</returns>
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
