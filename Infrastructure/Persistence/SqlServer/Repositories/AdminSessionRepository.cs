using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AdminSessionRepository : GenericRepository<AdminSession, Guid>, IAdminSessionRepository
{
    public AdminSessionRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<List<AdminSession>> GetActiveSessionsAsync(Guid adminUserId)
    {
        return await _context.AdminSessions
            .Where(s => s.AdminUserId == adminUserId && s.LogoutAt == null)
            .OrderByDescending(s => s.LoginAt)
            .ToListAsync();
    }

    public async Task CreateAsync(AdminSession session)
    {
        await _context.AdminSessions.AddAsync(session);
    }
}
