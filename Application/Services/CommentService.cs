using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Constants;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio para gestionar los comentarios y respuestas en publicaciones.
/// </summary>
public class CommentService(
    IUnitOfWork unitOfWork,
    IPostService postService,
    ILogger<CommentService> logger) : ICommentService
{
    /// <summary>
    /// Crea un comentario en respuesta a una publicación padre existente.
    /// </summary>
    /// <param name="parentPostId">Identificador único de la publicación a la cual se está comentando.</param>
    /// <param name="userId">Identificador único del usuario autor del comentario.</param>
    /// <param name="model">Modelo de solicitud con el contenido de texto del comentario.</param>
    /// <returns>La información de la publicación/comentario creado en forma de DTO.</returns>
    public async Task<PostDto> CreateComment(Guid parentPostId, Guid userId, CreateCommentRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Creating comment for ParentPostId: {ParentPostId}, UserId: {UserId}", parentPostId, userId);
        }

        var parentPost = await unitOfWork.Posts.GetByIdAsync(parentPostId);
        if (parentPost is null || parentPost.DeletedAt is not null || !parentPost.IsPublished)
        {
            throw new ResourceNotFoundException("La publicación original no existe o no está disponible");
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);
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

        return await postService.Get(comment.PostId);
    }
}
