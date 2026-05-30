using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de reportes de contenido (denuncias), heredando operaciones genéricas para la entidad <see cref="ContentReport"/>.
/// </summary>
public interface IContentReportRepository : IGenericRepository<ContentReport, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona todos los reportes de contenido con estado pendiente, ordenados por prioridad y fecha.
    /// </summary>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista conteniendo los reportes pendientes <see cref="ContentReport"/>.</returns>
    Task<List<ContentReport>> GetPendingReportsAsync(int limit = 0, int offset = 0);

    /// <summary>
    /// Obtiene de forma asíncrona los reportes de contenido filtrados por su estado (ej. "pendiente", "resuelto", "descartado").
    /// </summary>
    /// <param name="status">El estado del reporte a filtrar.</param>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de reportes que coinciden con el estado.</returns>
    Task<List<ContentReport>> GetByStatusAsync(string status, int limit = 0, int offset = 0);

    /// <summary>
    /// Obtiene de forma asíncrona todos los reportes de contenido aplicados contra una entidad específica (ej. "Post", "User", "Message").
    /// </summary>
    /// <param name="entityType">El tipo de la entidad reportada.</param>
    /// <param name="entityId">El identificador único de la entidad reportada.</param>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de reportes aplicados contra la entidad.</returns>
    Task<List<ContentReport>> GetByEntityAsync(string entityType, Guid entityId, int limit = 0, int offset = 0);

    /// <summary>
    /// Verifica de forma asíncrona si existe una denuncia activa y pendiente realizada por el mismo usuario contra la misma entidad.
    /// </summary>
    /// <param name="reporterUserId">Identificador del usuario denunciante.</param>
    /// <param name="entityType">El tipo de la entidad denunciada.</param>
    /// <param name="entityId">El identificador único de la entidad denunciada.</param>
    /// <returns>True si el usuario ya cuenta con un reporte pendiente activo sobre esa entidad; de lo contrario, False.</returns>
    Task<bool> HasActiveReportAsync(Guid reporterUserId, string entityType, Guid entityId);
}