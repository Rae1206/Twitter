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

    public List<Post> GetAll(int limit, int offset, Guid? userId = null, bool? isPublished = null)
    {
        var query = _context.Posts.AsQueryable();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return query.Skip(normalizedOffset).Take(normalizedLimit).ToList();
    }

    public List<Post> GetPostsByUserId(Guid userId, int limit = 0, int offset = 0)
    {
        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return _context.Posts
            .Where(p => p.UserId == userId)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();
    }
}
