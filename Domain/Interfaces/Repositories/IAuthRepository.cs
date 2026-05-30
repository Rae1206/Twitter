using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de autenticación, heredando operaciones genéricas para la entidad <see cref="User"/>.
/// </summary>
public interface IAuthRepository : IGenericRepository<User, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona un usuario mediante su dirección de correo electrónico, incluyendo información relacionada para la autenticación.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del usuario.</param>
    /// <returns>La entidad de usuario encontrada, o null si no se encuentra registrado.</returns>
    Task<User?> GetByEmailAsync(string email);
}
