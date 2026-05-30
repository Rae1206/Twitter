using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de gestión de usuarios, heredando de <see cref="IGenericRepository{User, Guid}"/>.
/// </summary>
public interface IUserRepository : IGenericRepository<User, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona un listado paginado de usuarios activos en el sistema, permitiendo filtros opcionales de nickname o correo electrónico.
    /// Incluye la carga asociativa de sus roles correspondientes.
    /// </summary>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="nickname">Filtro opcional para buscar por nickname (nombre de usuario).</param>
    /// <param name="email">Filtro opcional para buscar por correo electrónico.</param>
    /// <returns>Una lista conteniendo los usuarios <see cref="User"/>.</returns>
    Task<List<User>> GetAllAsync(int limit, int offset, string? nickname = null, string? email = null);

    /// <summary>
    /// Obtiene de forma asíncrona un usuario mediante su dirección de correo electrónico única.
    /// </summary>
    /// <param name="email">La dirección de correo electrónico del usuario.</param>
    /// <returns>La entidad de usuario <see cref="User"/> o null si no se encuentra registrado.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Verifica de forma asíncrona si existe en el sistema algún usuario registrado con el correo electrónico especificado.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico a verificar.</param>
    /// <returns>True si el correo está registrado; de lo contrario, False.</returns>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Obtiene de forma asíncrona el hash de la contraseña de un usuario mediante su identificador único.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La cadena hash de la contraseña o null si el usuario no existe.</returns>
    Task<string?> GetPasswordHashAsync(Guid userId);
}
