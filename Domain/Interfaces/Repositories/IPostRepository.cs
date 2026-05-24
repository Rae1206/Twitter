using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IPostRepository : IGenericRepository<Post, Guid>
{
    Task<List<Post>> GetAllAsync(int limit, int offset, Guid? userId = null, bool? isPublished = null);
    Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int limit = 0, int offset = 0);
    IQueryable<Post> GetQueryable();

    /// <summary>
    /// Devuelve posts efímeros ya expirados (ExpiresAt &lt;= cutoff) que todavía NO fueron soft-deleted.
    /// Usa IgnoreQueryFilters para evadir el filtro global que oculta posts vencidos del feed.
    /// </summary>
    Task<List<Post>> GetExpiredPendingSoftDeleteAsync(DateTime cutoff, int batchSize);
}
