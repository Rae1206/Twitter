using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de posts.
/// Hereda de IGenericRepository para lectura.
/// </summary>
public interface IPostRepository : IGenericRepository<Post, Guid>
{
    List<Post> GetAll(int limit, int offset, Guid? userId = null, bool? isPublished = null);
    List<Post> GetPostsByUserId(Guid userId, int limit = 0, int offset = 0);
}
