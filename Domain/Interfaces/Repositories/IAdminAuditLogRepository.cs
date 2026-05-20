using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IAdminAuditLogRepository : IGenericRepository<AdminAuditLog, Guid>
{
    Task<List<AdminAuditLog>> GetPagedAsync(int limit, int offset, Guid? adminUserId = null, string? action = null, string? entityType = null, DateTime? dateFrom = null, DateTime? dateTo = null);
}
