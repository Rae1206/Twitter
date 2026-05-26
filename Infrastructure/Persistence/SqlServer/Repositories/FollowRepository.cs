using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de follows.
/// </summary>
public class FollowRepository : GenericRepository<Follow, Guid>, IFollowRepository
{
    public FollowRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<Follow?> GetFollow(Guid followerId, Guid followingId)
    {
        return await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<List<User>> GetFollowers(Guid userId, int limit = 0, int offset = 0)
    {
        var query = _context.Follows
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<List<User>> GetFollowing(Guid userId, int limit = 0, int offset = 0)
    {
        var query = _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<int> GetFollowersCount(Guid userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCount(Guid userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowerId == userId);
    }

    public async Task<bool> IsFollowing(Guid followerId, Guid followingId)
    {
        return await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }
}