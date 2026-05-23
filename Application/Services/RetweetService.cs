using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class RetweetService(
    IUnitOfWork unitOfWork,
    IPostService postService,
    ILogger<RetweetService> logger) : IRetweetService
{
    public async Task<PostDto> CreateRetweet(Guid postId, Guid userId, CreateRetweetRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Creating retweet for PostId: {PostId}, UserId: {UserId}", postId, userId);
        }

        var targetPost = unitOfWork.Posts.GetById(postId);
        if (targetPost is null || targetPost.DeletedAt is not null || !targetPost.IsPublished)
        {
            throw new ResourceNotFoundException("La publicación original no existe o no está disponible");
        }

        var user = unitOfWork.Users.GetById(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("user", userId);
        }

        // Resolve pure retweet to its source post to prevent recursive depth
        var currentPost = targetPost;
        while (currentPost.RetweetOfPostId.HasValue && string.IsNullOrWhiteSpace(currentPost.Content))
        {
            var parent = unitOfWork.Posts.GetById(currentPost.RetweetOfPostId.Value);
            if (parent is null || parent.DeletedAt is not null || !parent.IsPublished)
            {
                break;
            }
            currentPost = parent;
        }
        var resolvedPostId = currentPost.PostId;

        // Duplicate prevention for pure retweets
        if (string.IsNullOrWhiteSpace(model.Content))
        {
            var alreadyRetweeted = await unitOfWork.Posts.IfExists(p => p.UserId == userId 
                && p.RetweetOfPostId == resolvedPostId 
                && (p.Content == null || p.Content == string.Empty || p.Content.Trim() == string.Empty));

            if (alreadyRetweeted)
            {
                throw new BadRequestException(ErrorConstants.ALREADY_RETWEETED);
            }
        }

        var retweet = new Post
        {
            PostId = Guid.NewGuid(),
            UserId = userId,
            Content = model.Content ?? string.Empty,
            RetweetOfPostId = resolvedPostId,
            IsPublished = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(retweet);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Retweet created successfully with PostId: {PostId}", retweet.PostId);
        }

        return postService.Get(retweet.PostId);
    }
}
