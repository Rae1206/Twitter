using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface ISystemConfigRepository : IGenericRepository<SystemConfig, Guid>
{
    Task<SystemConfig?> GetByKeyAsync(string key);
    Task<List<SystemConfig>> GetAllEditableAsync();
}
