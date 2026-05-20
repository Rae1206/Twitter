using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IContentReportRepository : IGenericRepository<ContentReport, Guid>
{
    Task<List<ContentReport>> GetPendingReportsAsync(int limit = 0, int offset = 0);
    Task<List<ContentReport>> GetByStatusAsync(string status, int limit = 0, int offset = 0);
}
