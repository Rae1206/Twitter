using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IContentReportRepository : IGenericRepository<ContentReport, Guid>
{
    Task<List<ContentReport>> GetPendingReportsAsync(int limit = 0, int offset = 0);
    Task<List<ContentReport>> GetByStatusAsync(string status, int limit = 0, int offset = 0);
    Task<List<ContentReport>> GetByEntityAsync(string entityType, Guid entityId, int limit = 0, int offset = 0);
    Task<bool> HasActiveReportAsync(Guid reporterUserId, string entityType, Guid entityId);
}