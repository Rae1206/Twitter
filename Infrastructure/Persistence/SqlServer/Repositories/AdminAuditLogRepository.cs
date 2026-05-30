using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para la persistencia y consulta avanzada de registros de auditoría administrativa.
/// </summary>
public class AdminAuditLogRepository : GenericRepository<AdminAuditLog, Guid>, IAdminAuditLogRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AdminAuditLogRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public AdminAuditLogRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona un listado paginado y filtrado de registros de auditoría administrativa, ordenados descendentemente por fecha de creación.
    /// </summary>
    /// <param name="limit">Cantidad máxima de registros a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="adminUserId">Identificador opcional del administrador autor de las acciones.</param>
    /// <param name="action">Nombre opcional de la acción realizada.</param>
    /// <param name="entityType">Tipo opcional de la entidad afectada.</param>
    /// <param name="dateFrom">Fecha de inicio opcional del rango de búsqueda.</param>
    /// <param name="dateTo">Fecha de fin opcional del rango de búsqueda.</param>
    /// <returns>Una lista conteniendo los registros de auditoría de administración <see cref="AdminAuditLog"/> resultantes.</returns>
    public async Task<List<AdminAuditLog>> GetPagedAsync(int limit, int offset, Guid? adminUserId = null, string? action = null, string? entityType = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.AdminAuditLogs.AsQueryable();

        if (adminUserId.HasValue)
            query = query.Where(a => a.AdminUserId == adminUserId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (dateFrom.HasValue)
            query = query.Where(a => a.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.CreatedAt <= dateTo.Value);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }
}
