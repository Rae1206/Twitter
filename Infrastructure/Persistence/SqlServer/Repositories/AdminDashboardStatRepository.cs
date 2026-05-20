using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AdminDashboardStatRepository : GenericRepository<AdminDashboardStat, Guid>, IAdminDashboardStatRepository
{
    public AdminDashboardStatRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<List<AdminDashboardStat>> GetAllAsync()
    {
        return await _context.AdminDashboardStats.ToListAsync();
    }

    public async Task UpsertAsync(AdminDashboardStat stat)
    {
        var existing = await _context.AdminDashboardStats.FirstOrDefaultAsync(s => s.StatKey == stat.StatKey);
        if (existing is not null)
        {
            existing.StatValue = stat.StatValue;
            existing.LastCalculated = stat.LastCalculated;
            _context.Update(existing);
        }
        else
        {
            _context.Add(stat);
        }
    }
}
