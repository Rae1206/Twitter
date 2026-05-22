using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

public class PostMediaRepository : GenericRepository<PostMedia, Guid>, IPostMediaRepository
{
    public PostMediaRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<List<PostMedia>> GetOrphansOlderThanAsync(DateTime cutoff)
    {
        return await _context.Set<PostMedia>()
            .Where(m => m.PostId == null && m.CreatedAt < cutoff)
            .ToListAsync();
    }

    public async Task<List<PostMedia>> GetByPostIdAsync(Guid postId)
    {
        return await _context.Set<PostMedia>()
            .Where(m => m.PostId == postId)
            .ToListAsync();
    }
}
