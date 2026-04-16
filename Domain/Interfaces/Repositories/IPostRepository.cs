using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de posts.
/// </summary>
public interface IPostRepository : IGenericRepository<Post, Guid>
{
    // GetAll con filtros adicionales
    List<Post> GetAll(int limit, int offset, Guid? userId = null, bool? isPublished = null);
    
    // Método específico de Post
    bool ChangeStatus(Guid postId, bool isPublished);
}