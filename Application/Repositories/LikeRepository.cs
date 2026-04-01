using Application.Interfaces.Repositories;
using Application.Models.DTOs;

namespace Application.Repositories;

public class LikeRepository : ILikeRepository
{
    // TODO: Implement with EF Core

    public LikeDto Create(LikeDto like)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Delete(Guid userId, Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Exists(Guid userId, Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public LikeDto? GetById(Guid likeId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<LikeDto> GetLikesByPost(Guid postId, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public int GetLikesCount(Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }
}