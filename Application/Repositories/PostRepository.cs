using Application.Interfaces.Repositories;
using Application.Models.DTOs;

namespace Application.Repositories;

public class PostRepository : IPostRepository
{
    // TODO: Implement with Entity Framework Core
    // private readonly AppDbContext _context;

    public PostDto Create(PostDto post)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Delete(Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<PostDto> GetAll(int limit, int offset, Guid? userId, bool? isPublished)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public PostDto? GetById(Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public PostDto? Update(Guid postId, PostDto post)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool ChangeStatus(Guid postId, bool isPublished)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }
}