using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserSuspensionRepository : GenericRepository<UserSuspension, Guid>, IUserSuspensionRepository
{
    public UserSuspensionRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<UserSuspension?> GetActiveSuspensionAsync(Guid userId)
    {
        return await _context.UserSuspensions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
