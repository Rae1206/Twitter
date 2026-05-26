using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de follows.
/// Hereda de IGenericRepository para operaciones CRUD genéricas.
/// </summary>
public interface IFollowRepository : IGenericRepository<Follow, Guid>
{
    Task<Follow?> GetFollow(Guid followerId, Guid followingId);
    Task<List<User>> GetFollowers(Guid userId, int limit = 0, int offset = 0);
    Task<List<User>> GetFollowing(Guid userId, int limit = 0, int offset = 0);
    Task<int> GetFollowersCount(Guid userId);
    Task<int> GetFollowingCount(Guid userId);
    Task<bool> IsFollowing(Guid followerId, Guid followingId);
}