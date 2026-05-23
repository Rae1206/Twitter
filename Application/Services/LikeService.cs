using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class LikeService(
    IUnitOfWork unitOfWork,
    ILogger<LikeService> logger) : ILikeService
{
    public async Task ToggleLike(Guid postId, Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Toggling like for PostId: {PostId}, UserId: {UserId}", postId, userId);
        }

        var post = unitOfWork.Posts.GetById(postId);
        if (post is null)
        {
            throw new ResourceNotFoundException("post", postId);
        }

        var user = unitOfWork.Users.GetById(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("user", userId);
        }

        try
        {
            var existingLike = await unitOfWork.Likes.GetLike(userId, postId);
            if (existingLike is not null)
            {
                unitOfWork.Delete(existingLike);
            }
            else
            {
                var newLike = new Like
                {
                    LikeId = Guid.NewGuid(),
                    UserId = userId,
                    PostId = postId,
                    CreatedAt = DateTimeHelper.UtcNow()
                };
                unitOfWork.Create(newLike);
            }

            await unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Likes_UserId_PostId") == true || ex.InnerException?.Message.Contains("unique") == true)
        {
            throw new BadRequestException(ErrorConstants.ALREADY_LIKED);
        }
    }

    public async Task<List<User>> GetLikers(Guid postId, int limit = 0, int offset = 0)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Getting likers for PostId: {PostId}", postId);
        }

        var post = unitOfWork.Posts.GetById(postId);
        if (post is null)
        {
            throw new ResourceNotFoundException("post", postId);
        }

        return await unitOfWork.Likes.GetLikers(postId, limit, offset);
    }
}
