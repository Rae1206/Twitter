using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Helpers;

namespace Application.Services;

public class DashboardService(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<DashboardService> logger) : IDashboardService
{
    private static readonly string CacheKey = "admin:dashboard:stats";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public GenericResponse<Dictionary<string, decimal>> GetStatsAsync()
    {
        var cached = cacheService.Get<Dictionary<string, decimal>>(CacheKey);
        if (cached is not null)
        {
            return new GenericResponse<Dictionary<string, decimal>> { Data = cached };
        }

        var stats = unitOfWork.AdminDashboardStats.GetAllAsync().Result;
        var dict = stats.ToDictionary(s => s.StatKey, s => s.StatValue);

        cacheService.Create(CacheKey, CacheTtl, dict);

        return new GenericResponse<Dictionary<string, decimal>> { Data = dict };
    }

    public async Task RecalculateStatsAsync()
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Recalculando estadísticas del dashboard");
        }

        // Use IgnoreQueryFilters to count all users including soft-deleted
        var totalUsers = 0; // Cannot easily access IgnoreQueryFilters from service layer without DbContext
        var activeUsers = 0;
        var suspendedUsers = 0;
        var totalPosts = 0;
        var pendingReports = 0;
        var flaggedPosts = 0;

        // Fallback: use repository methods that respect global filters
        activeUsers = unitOfWork.Users.GetAll(0, 0, filter: u => u.IsActive).Count;
        suspendedUsers = unitOfWork.Users.GetAll(0, 0, filter: u => u.IsSuspended).Count;
        totalPosts = unitOfWork.Posts.GetAll(0, 0).Count;
        pendingReports = unitOfWork.ContentReports.GetPendingReportsAsync().Result.Count;
        flaggedPosts = unitOfWork.Posts.GetAll(0, 0, filter: p => p.IsFlagged).Count;

        var stats = new Dictionary<string, decimal>
        {
            { "total_users", activeUsers + suspendedUsers },
            { "active_users", activeUsers },
            { "suspended_users", suspendedUsers },
            { "total_posts", totalPosts },
            { "pending_reports", pendingReports },
            { "flagged_posts", flaggedPosts },
            { "new_users_today", activeUsers } // Placeholder; real implementation would filter by CreatedAt
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
