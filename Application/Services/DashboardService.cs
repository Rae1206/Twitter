using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Helpers;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio para calcular, recuperar y gestionar estadísticas del panel (dashboard) administrativo.
/// </summary>
public class DashboardService(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<DashboardService> logger) : IDashboardService
{
    private static readonly string CacheKey = "admin:dashboard:stats";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Recupera las estadísticas generales del sistema almacenadas en base de datos.
    /// Utiliza caché en memoria para optimizar lecturas sucesivas.
    /// </summary>
    /// <returns>Un sobre genérico con un diccionario que asocia la clave de métrica con su valor numérico.</returns>
    public async Task<GenericResponse<Dictionary<string, decimal>>> GetStatsAsync()
    {
        var cached = cacheService.Get<Dictionary<string, decimal>>(CacheKey);
        if (cached is not null)
        {
            return new GenericResponse<Dictionary<string, decimal>> { Data = cached };
        }

        var stats = await unitOfWork.AdminDashboardStats.GetAllAsync();
        var dict = stats.ToDictionary(s => s.StatKey, s => s.StatValue);

        cacheService.Create(CacheKey, CacheTtl, dict);

        return new GenericResponse<Dictionary<string, decimal>> { Data = dict };
    }

    /// <summary>
    /// Fuerza el recalculo manual de todas las estadísticas de la plataforma
    /// (usuarios, publicaciones, reportes, etc.) y actualiza los registros en base de datos e invalida la caché.
    /// </summary>
    /// <returns>Una tarea asíncrona que representa el proceso de recalculo.</returns>
    public async Task RecalculateStatsAsync()
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Recalculando estadísticas del dashboard");
        }

        var activeUsers = (await unitOfWork.Users.GetAllAsync(0, 0, filter: u => u.IsActive)).Count;
        var suspendedUsers = (await unitOfWork.Users.GetAllAsync(0, 0, filter: u => u.IsSuspended)).Count;
        var totalPosts = (await unitOfWork.Posts.GetAllAsync(0, 0)).Count;
        var pendingReports = (await unitOfWork.ContentReports.GetPendingReportsAsync()).Count;
        var flaggedPosts = (await unitOfWork.Posts.GetAllAsync(0, 0, filter: p => p.IsFlagged)).Count;

        var stats = new Dictionary<string, decimal>
        {
            { "total_users", activeUsers + suspendedUsers },
            { "active_users", activeUsers },
            { "suspended_users", suspendedUsers },
            { "total_posts", totalPosts },
            { "pending_reports", pendingReports },
            { "flagged_posts", flaggedPosts },
            { "new_users_today", activeUsers }
        };

        foreach (var kvp in stats)
        {
            await unitOfWork.AdminDashboardStats.UpsertAsync(new AdminDashboardStat
            {
                StatId = Guid.NewGuid(),
                StatKey = kvp.Key,
                StatValue = kvp.Value,
                LastCalculated = DateTimeHelper.UtcNow()
            });
        }

        await unitOfWork.SaveChangesAsync();

        cacheService.Delete(CacheKey);
    }
}
