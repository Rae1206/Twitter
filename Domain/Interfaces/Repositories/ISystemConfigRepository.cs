using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de configuraciones del sistema, heredando de <see cref="IGenericRepository{SystemConfig, Guid}"/>.
/// </summary>
public interface ISystemConfigRepository : IGenericRepository<SystemConfig, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona una clave de configuración del sistema buscando por su identificativo único.
    /// </summary>
    /// <param name="key">Nombre único de la clave de configuración.</param>
    /// <returns>La entidad de configuración <see cref="SystemConfig"/> o null si no existe.</returns>
    Task<SystemConfig?> GetByKeyAsync(string key);

    /// <summary>
    /// Obtiene de forma asíncrona todas las configuraciones editables del sistema.
    /// </summary>
    /// <returns>Una lista de todas las entidades <see cref="SystemConfig"/>.</returns>
    Task<List<SystemConfig>> GetAllEditableAsync();
}
