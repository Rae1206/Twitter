using Application.Interfaces.Repositories;
using Application.Models.DTOs;

namespace Application.Repositories;

public class RetweetRepository : IRetweetRepository
{
    // TODO: Implement with EF Core

    public RetweetDto Create(RetweetDto retweet)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Delete(Guid retweetId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Exists(Guid userId, Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public RetweetDto? GetById(Guid retweetId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<RetweetDto> GetRetweetsByPost(Guid postId, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<RetweetDto> GetRetweetsByUser(Guid userId, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public int GetRetweetsCount(Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }
}