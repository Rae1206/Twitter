using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de estadísticas del panel de administración, heredando operaciones genéricas para la entidad <see cref="AdminDashboardStat"/>.
/// </summary>
public interface IAdminDashboardStatRepository : IGenericRepository<AdminDashboardStat, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona todas las estadísticas guardadas del panel de control.
    /// </summary>
    /// <returns>Una lista conteniendo todas las entidades <see cref="AdminDashboardStat"/>.</returns>
    Task<List<AdminDashboardStat>> GetAllAsync();

    /// <summary>
    /// Realiza de forma asíncrona un Upsert (actualiza el valor si existe, o inserta uno nuevo si no existe) para una estadística clave específica.
    /// </summary>
    /// <param name="stat">La entidad conteniendo la estadística a persistir.</param>
    Task UpsertAsync(AdminDashboardStat stat);
}
