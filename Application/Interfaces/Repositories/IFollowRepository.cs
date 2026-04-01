using Application.Models.DTOs;

namespace Application.Interfaces.Repositories;

public interface IFollowRepository
{
    FollowDto Create(FollowDto follow);
    bool Delete(Guid followerId, Guid followingId);
    FollowDto? GetById(Guid followId);
    List<FollowDto> GetFollowers(Guid userId, int limit, int offset);
    List<FollowDto> GetFollowing(Guid userId, int limit, int offset);
    bool Exists(Guid followerId, Guid followingId);
    int GetFollowersCount(Guid userId);
    int GetFollowingCount(Guid userId);
}