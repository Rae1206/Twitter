using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IAdminDashboardStatRepository : IGenericRepository<AdminDashboardStat, Guid>
{
    Task<List<AdminDashboardStat>> GetAllAsync();
    Task UpsertAsync(AdminDashboardStat stat);
}
