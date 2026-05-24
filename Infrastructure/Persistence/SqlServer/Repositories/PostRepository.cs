using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de posts - solo lectura.
/// </summary>
public class PostRepository : GenericRepository<Post, Guid>, IPostRepository
{
    public PostRepository(TwitterDbContext context) : base(context)
    {
    }

    public IQueryable<Post> GetQueryable()
    {
        return _context.Posts.AsQueryable();
    }

    public async Task<List<Post>> GetAllAsync(int limit, int offset, Guid? userId = null, bool? isPublished = null)
    {
        var query = _context.Posts.AsQueryable();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int limit = 0, int offset = 0)
    {
        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await _context.Posts
            .Where(p => p.UserId == userId)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }

    public async Task<List<Post>> GetExpiredPendingSoftDeleteAsync(DateTime cutoff, int batchSize)
    {
        // IgnoreQueryFilters() hace bypass del filtro global; necesario porque el filtro YA oculta
        // posts vencidos. El cleanup necesita verlos para marcar DeletedAt.
        return await _context.Posts
            .IgnoreQueryFilters()
            .Where(p => p.DeletedAt == null
                        && p.ExpiresAt != null
                        && p.ExpiresAt <= cutoff)
            .OrderBy(p => p.ExpiresAt)
            .Take(batchSize)
            .ToListAsync();
    }
}
