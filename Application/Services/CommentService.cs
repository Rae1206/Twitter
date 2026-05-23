using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CommentService(
    IUnitOfWork unitOfWork,
    IPostService postService,
    ILogger<CommentService> logger) : ICommentService
{
    public async Task<PostDto> CreateComment(Guid parentPostId, Guid userId, CreateCommentRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Creating comment for ParentPostId: {ParentPostId}, UserId: {UserId}", parentPostId, userId);
        }

        var parentPost = unitOfWork.Posts.GetById(parentPostId);
        if (parentPost is null || parentPost.DeletedAt is not null || !parentPost.IsPublished)
        {
            throw new ResourceNotFoundException("La publicación original no existe o no está disponible");
        }

        var user = unitOfWork.Users.GetById(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("user", userId);
        }

        var comment = new Post
        {
            PostId = Guid.NewGuid(),
            UserId = userId,
            Content = model.Content,
            RepliedToPostId = parentPostId,
            IsPublished = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(comment);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Comment created successfully with PostId: {PostId}", comment.PostId);
        }

        return postService.Get(comment.PostId);
    }
}
