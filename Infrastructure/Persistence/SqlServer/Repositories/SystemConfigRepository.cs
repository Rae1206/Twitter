using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SystemConfigRepository : GenericRepository<SystemConfig, Guid>, ISystemConfigRepository
{
    public SystemConfigRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<SystemConfig?> GetByKeyAsync(string key)
    {
        return await _context.SystemConfigs.FirstOrDefaultAsync(c => c.Key == key);
    }

    public async Task<List<SystemConfig>> GetAllEditableAsync()
    {
        return await _context.SystemConfigs.ToListAsync();
    }
}
