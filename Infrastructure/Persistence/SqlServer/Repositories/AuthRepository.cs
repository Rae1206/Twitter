using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio específico de autenticación para realizar búsquedas optimizadas de usuarios junto con sus roles.
/// </summary>
public class AuthRepository : GenericRepository<User, Guid>, IAuthRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AuthRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public AuthRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona un usuario mediante su dirección de correo electrónico cargando en lote su lista de roles asociados.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del usuario.</param>
    /// <returns>La entidad <see cref="User"/> junto con sus roles cargados, o null si no se encuentra registrado.</returns>
    public async Task<User?> GetByEmailAsync(string email) => await _context.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Email == email);
}
