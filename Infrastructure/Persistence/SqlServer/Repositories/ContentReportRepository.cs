using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos encargado del almacenamiento, clasificación, estados y recuperación de las denuncias de contenido.
/// </summary>
public class ContentReportRepository : GenericRepository<ContentReport, Guid>, IContentReportRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ContentReportRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public ContentReportRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona todos los reportes de contenido con estado pendiente, ordenados descendentemente por prioridad y cronológicamente.
    /// </summary>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de reportes de contenido pendientes <see cref="ContentReport"/>.</returns>
    public async Task<List<ContentReport>> GetPendingReportsAsync(int limit = 0, int offset = 0)
    {
        var query = _context.ContentReports
            .Where(r => r.Status == "pending")
            .OrderByDescending(r => r.Priority).ThenByDescending(r => r.CreatedAt);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona todos los reportes que coincidan con un estado particular, ordenados por prioridad y fecha.
    /// </summary>
    /// <param name="status">Nombre del estado a buscar.</param>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista de reportes <see cref="ContentReport"/>.</returns>
    public async Task<List<ContentReport>> GetByStatusAsync(string status, int limit = 0, int offset = 0)
    {
        var query = _context.ContentReports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.Priority).ThenByDescending(r => r.CreatedAt);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asíncrona todos los reportes individuales aplicados contra una entidad específica, de más reciente a más antiguo.
    /// </summary>
    /// <param name="entityType">El tipo de la entidad.</param>
    /// <param name="entityId">El identificador único de la entidad.</param>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista conteniendo los reportes de la entidad.</returns>
    public async Task<List<ContentReport>> GetByEntityAsync(string entityType, Guid entityId, int limit = 0, int offset = 0)
    {
        var query = _context.ContentReports
            .Where(r => r.EntityType == entityType && r.EntityId == entityId)
            .OrderByDescending(r => r.CreatedAt);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    /// <summary>
    /// Verifica de forma asíncrona si existe un reporte activo y pendiente del mismo usuario contra la misma entidad.
    /// </summary>
    /// <param name="reporterUserId">Identificador del usuario denunciante.</param>
    /// <param name="entityType">El tipo de la entidad denunciada.</param>
    /// <param name="entityId">El identificador único de la entidad denunciada.</param>
    /// <returns>True si el usuario ya cuenta con un reporte pendiente activo sobre esa entidad; de lo contrario, False.</returns>
    public async Task<bool> HasActiveReportAsync(Guid reporterUserId, string entityType, Guid entityId)
    {
        return await _context.ContentReports.AnyAsync(r =>
            r.ReporterUserId == reporterUserId &&
            r.EntityType == entityType &&
            r.EntityId == entityId &&
            (r.Status == "pending"));
    }
}