using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de registros de auditoría de administración, heredando operaciones genéricas para la entidad <see cref="AdminAuditLog"/>.
/// </summary>
public interface IAdminAuditLogRepository : IGenericRepository<AdminAuditLog, Guid>
{
    /// <summary>
    /// Obtiene de forma asíncrona un listado paginado y filtrado de registros de auditoría administrativa.
    /// Permite aplicar filtros por administrador, acción ejecutada, tipo de entidad afectada y rango de fechas.
    /// </summary>
    /// <param name="limit">Cantidad máxima de registros a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="adminUserId">Identificador opcional del administrador autor de las acciones.</param>
    /// <param name="action">Nombre opcional de la acción a buscar.</param>
    /// <param name="entityType">Tipo de entidad opcional afectada por la acción.</param>
    /// <param name="dateFrom">Fecha de inicio opcional para el rango de búsqueda.</param>
    /// <param name="dateTo">Fecha de fin opcional para el rango de búsqueda.</param>
    /// <returns>Una lista de entidades <see cref="AdminAuditLog"/> que cumplen los criterios especificados.</returns>
    Task<List<AdminAuditLog>> GetPagedAsync(int limit, int offset, Guid? adminUserId = null, string? action = null, string? entityType = null, DateTime? dateFrom = null, DateTime? dateTo = null);
}
