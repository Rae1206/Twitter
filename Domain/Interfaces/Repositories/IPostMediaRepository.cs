using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IPostMediaRepository : IGenericRepository<PostMedia, Guid>
{
    Task<List<PostMedia>> GetOrphansOlderThanAsync(DateTime cutoff);
    Task<List<PostMedia>> GetByPostIdAsync(Guid postId);
}
