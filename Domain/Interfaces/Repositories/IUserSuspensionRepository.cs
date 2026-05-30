using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de suspensiones de usuarios, heredando de <see cref="IGenericRepository{UserSuspension, Guid}"/>.
/// </summary>
public interface IUserSuspensionRepository : IGenericRepository<UserSuspension, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona la suspensión activa (no expirada y marcada como activa) de un usuario específico.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>La entidad de suspensión <see cref="UserSuspension"/> activa o null si no posee suspensiones vigentes.</returns>
    Task<UserSuspension?> GetActiveSuspensionAsync(Guid userId);
}
