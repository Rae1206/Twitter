using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para el almacenamiento, consulta, filtros complejos, inyección de roles y verificación de credenciales de los usuarios.
/// </summary>
public class UserRepository : GenericRepository<User, Guid>, IUserRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UserRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public UserRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona un usuario individual por su identificador único cargando asociativamente sus roles del sistema.
    /// </summary>
    /// <param name="id">Identificador único del usuario.</param>
    /// <returns>La entidad de usuario <see cref="User"/> junto con sus roles inyectados, o null si no existe.</returns>
    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista paginada y filtrada de usuarios inyectándoles sus roles asociados.
    /// </summary>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="nickname">Filtro condicional por apodo.</param>
    /// <param name="email">Filtro condicional por correo electrónico.</param>
    /// <returns>Una lista conteniendo las entidades de usuarios <see cref="User"/>.</returns>
    public async Task<List<User>> GetAllAsync(int limit, int offset, string? nickname = null, string? email = null)
    {
        var query = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nickname))
            query = query.Where(u => u.Nickname.Contains(nickname));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.Contains(email));

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona un usuario mediante su dirección de correo electrónico única.
    /// </summary>
    /// <param name="email">El correo electrónico del usuario.</param>
    /// <returns>La entidad de usuario <see cref="User"/> o null si no se encuentra registrado.</returns>
    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    /// <summary>
    /// Verifica de forma asíncrona si existe algún usuario activo registrado en el sistema con el correo especificado.
    /// </summary>
    /// <param name="email">El correo a buscar.</param>
    /// <returns>True si el correo ya se encuentra registrado; de lo contrario, False.</returns>
    public async Task<bool> ExistsByEmailAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);

    /// <summary>
    /// Obtiene de forma asíncrona la cadena hash de la contraseña de un usuario determinado.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La cadena hash de la contraseña del usuario, o null si no se encuentra.</returns>
    public async Task<string?> GetPasswordHashAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.PasswordHash;
    }
}
