using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Models.DTOs;

namespace Application.Interfaces.Services;

/// <summary>
/// Interfaz del servicio de follows.
/// </summary>
public interface IFollowService
{
    Task FollowUser(Guid followerId, Guid followingId);
    Task UnfollowUser(Guid followerId, Guid followingId);
    Task<List<UserDto>> GetFollowers(Guid userId, int limit = 0, int offset = 0);
    Task<List<UserDto>> GetFollowing(Guid userId, int limit = 0, int offset = 0);
    Task<int> GetFollowersCount(Guid userId);
    Task<int> GetFollowingCount(Guid userId);
    Task<bool> IsFollowing(Guid followerId, Guid followingId);
}
