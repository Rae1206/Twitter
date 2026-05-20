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
        var query = _context.ContentReports.Where(r => r.Status == "Pending");

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }

    public async Task<List<ContentReport>> GetByStatusAsync(string status, int limit = 0, int offset = 0)
    {
        var query = _context.ContentReports.Where(r => r.Status == status);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }
}
