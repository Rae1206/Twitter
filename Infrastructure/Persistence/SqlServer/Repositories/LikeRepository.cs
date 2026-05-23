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
/// Repositorio de likes.
/// </summary>
public class LikeRepository : GenericRepository<Like, Guid>, ILikeRepository
{
    public LikeRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<Like?> GetLike(Guid userId, Guid postId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
    }

    public async Task<List<User>> GetLikers(Guid postId, int limit = 0, int offset = 0)
    {
        var query = _context.Likes
            .Where(l => l.PostId == postId)
            .Select(l => l.User);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }
}
