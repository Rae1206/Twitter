using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IAdminSessionRepository : IGenericRepository<AdminSession, Guid>
{
    Task<List<AdminSession>> GetActiveSessionsAsync(Guid adminUserId);
    Task CreateAsync(AdminSession session);
}
