using Application.Interfaces.Repositories;
using Application.Models.DTOs;

namespace Application.Repositories;

public class FollowRepository : IFollowRepository
{
    // TODO: Implement with EF Core

    public FollowDto Create(FollowDto follow)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Delete(Guid followerId, Guid followingId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Exists(Guid followerId, Guid followingId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public FollowDto? GetById(Guid followId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<FollowDto> GetFollowers(Guid userId, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public int GetFollowersCount(Guid userId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<FollowDto> GetFollowing(Guid userId, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public int GetFollowingCount(Guid userId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }
}