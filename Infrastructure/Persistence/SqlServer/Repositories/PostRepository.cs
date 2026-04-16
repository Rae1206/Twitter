using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de posts.
/// </summary>
public class PostRepository : GenericRepository<Post, Guid>, IPostRepository
{
    public PostRepository(TwitterDbContext context) : base(context)
    {
    }

    public override Post? GetById(Guid id)
    {
        return _dbSet.Include(p => p.User).FirstOrDefault(p => p.PostId == id);
    }

    public new List<Post> GetAll(int limit, int offset, Guid? userId = null, bool? isPublished = null)
    {
        var query = _dbSet.Include(p => p.User).AsQueryable();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return query
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();
    }

    public override Post? Update(Guid id, Post post)
    {
        var entity = _dbSet.Find(id);
        if (entity is null) return null;

        entity.Content = post.Content;
        entity.IsPublished = post.IsPublished;

        if (post.UserId != entity.UserId)
            entity.UserId = post.UserId;

        return entity;
    }

    public bool ChangeStatus(Guid postId, bool isPublished)
    {
        var entity = _dbSet.Find(postId);
        if (entity is null) return false;

        entity.IsPublished = isPublished;
        return true;
    }
}