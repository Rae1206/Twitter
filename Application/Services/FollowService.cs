using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Shared.Constants;
using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class FollowService(
    IUnitOfWork unitOfWork,
    ILogger<FollowService> logger) : IFollowService
{
    public async Task FollowUser(Guid followerId, Guid followingId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User {FollowerId} following {FollowingId}", followerId, followingId);
        }

        if (followerId == followingId)
        {
            throw new BadRequestException("No puedes seguirte a ti mismo");
        }

        var follower = await unitOfWork.Users.GetByIdAsync(followerId);
        if (follower is null)
        {
            throw new ResourceNotFoundException("user", followerId);
        }

        var following = await unitOfWork.Users.GetByIdAsync(followingId);
        if (following is null)
        {
            throw new ResourceNotFoundException("user", followingId);
        }

        var existing = await unitOfWork.Follows.GetFollow(followerId, followingId);
        if (existing is not null)
        {
            throw new ConflictException(ErrorConstants.ALREADY_FOLLOWING);
        }

        try
        {
            var follow = new Follow
            {
                FollowId = Guid.NewGuid(),
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTimeHelper.UtcNow()
            };
            unitOfWork.Create(follow);

            // Update denormalized counts
            follower.FollowingCount++;
            following.FollowersCount++;
            unitOfWork.Update(follower);
            unitOfWork.Update(following);

            await unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Follows_FollowerId_FollowingId") == true || ex.InnerException?.Message.Contains("unique") == true)
        {
            throw new ConflictException(ErrorConstants.ALREADY_FOLLOWING);
        }
    }

    public async Task UnfollowUser(Guid followerId, Guid followingId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User {FollowerId} unfollowing {FollowingId}", followerId, followingId);
        }

        var follow = await unitOfWork.Follows.GetFollow(followerId, followingId);
        if (follow is null)
        {
            throw new BadRequestException(ErrorConstants.NOT_FOLLOWING);
        }

        var follower = await unitOfWork.Users.GetByIdAsync(followerId);
        var following = await unitOfWork.Users.GetByIdAsync(followingId);

        unitOfWork.Delete(follow);

        // Update denormalized counts
        if (follower is not null)
        {
            follower.FollowingCount = Math.Max(0, follower.FollowingCount - 1);
            unitOfWork.Update(follower);
        }
        if (following is not null)
        {
            following.FollowersCount = Math.Max(0, following.FollowersCount - 1);
            unitOfWork.Update(following);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<List<UserDto>> GetFollowers(Guid userId, int limit = 0, int offset = 0)
    {
        var users = await unitOfWork.Follows.GetFollowers(userId, limit, offset);
        return users.Select(MapToDto).ToList();
    }

    public async Task<List<UserDto>> GetFollowing(Guid userId, int limit = 0, int offset = 0)
    {
        var users = await unitOfWork.Follows.GetFollowing(userId, limit, offset);
        return users.Select(MapToDto).ToList();
    }

    public async Task<int> GetFollowersCount(Guid userId)
    {
        return await unitOfWork.Follows.GetFollowersCount(userId);
    }

    public async Task<int> GetFollowingCount(Guid userId)
    {
        return await unitOfWork.Follows.GetFollowingCount(userId);
    }

    public async Task<bool> IsFollowing(Guid followerId, Guid followingId)
    {
        return await unitOfWork.Follows.IsFollowing(followerId, followingId);
    }

    private static UserDto MapToDto(User user) => new()
    {
        UserId = user.UserId,
        Nickname = user.Nickname,
        Email = user.Email,
        Biography = user.Biography,
        ProfilePhotoUrl = user.ProfilePhotoUrl,
        IsActive = user.IsActive,
        IsSuspended = user.IsSuspended,
        IsShadowBanned = user.IsShadowBanned,
        DeletedAt = user.DeletedAt,
        FollowersCount = user.FollowersCount,
        FollowingCount = user.FollowingCount,
        CreatedAt = user.CreatedAt
    };
}