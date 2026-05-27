using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ContentReportRepository : GenericRepository<ContentReport, Guid>, IContentReportRepository
{
    public ContentReportRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<List<ContentReport>> GetPendingReportsAsync(int limit = 0, int offset = 0)
    {
        var query = _context.ContentReports
            .Where(r => r.Status == "pending")
            .OrderByDescending(r => r.Priority).ThenByDescending(r => r.CreatedAt);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<List<ContentReport>> GetByStatusAsync(string status, int limit = 0, int offset = 0)
    {
        var query = _context.ContentReports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.Priority).ThenByDescending(r => r.CreatedAt);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<List<ContentReport>> GetByEntityAsync(string entityType, Guid entityId, int limit = 0, int offset = 0)
    {
        var query = _context.ContentReports
            .Where(r => r.EntityType == entityType && r.EntityId == entityId)
            .OrderByDescending(r => r.CreatedAt);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<bool> HasActiveReportAsync(Guid reporterUserId, string entityType, Guid entityId)
    {
        return await _context.ContentReports.AnyAsync(r =>
            r.ReporterUserId == reporterUserId &&
            r.EntityType == entityType &&
            r.EntityId == entityId &&
            (r.Status == "pending"));
    }
}