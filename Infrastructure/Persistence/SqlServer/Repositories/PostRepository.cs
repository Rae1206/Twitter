using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de posts.
/// </summary>
public class PostRepository : GenericRepository<Post, Guid>, IPostRepository
{
    public PostRepository(TwitterDbContext context) : base(context)
    {
    }

    public override Post? GetById(Guid id) =>
        GetByField(p => p.PostId == id);

    public List<Post> GetAll(int limit, int offset, Guid? userId = null, bool? isPublished = null)
    {
        Expression<Func<Post, bool>> filter = p => 
            (userId == null || p.UserId == userId) 
            && (isPublished == null || p.IsPublished == isPublished);

        return base.GetAll(limit, offset, filter);
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