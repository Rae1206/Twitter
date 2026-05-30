using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Shared.Constants;
using Shared.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio encargado de gestionar las reacciones "Me gusta" (likes) en las publicaciones.
/// </summary>
public class LikeService(
    IUnitOfWork unitOfWork,
    ILogger<LikeService> logger) : ILikeService
{
    /// <summary>
    /// Alterna la reacción "Me gusta" (like) de un usuario en una publicación (lo agrega si no existe, o lo quita si ya existía).
    /// </summary>
    /// <param name="postId">Identificador único de la publicación.</param>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task ToggleLike(Guid postId, Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Toggling like for PostId: {PostId}, UserId: {UserId}", postId, userId);
        }

        var post = await unitOfWork.Posts.GetByIdAsync(postId);
        if (post is null)
        {
            throw new ResourceNotFoundException("post", postId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);
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

    /// <summary>
    /// Obtiene la lista completa de usuarios que han dado "Me gusta" a una publicación específica.
    /// </summary>
    /// <param name="postId">Identificador único de la publicación.</param>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista de usuarios que dieron "Me gusta".</returns>
    public async Task<List<User>> GetLikers(Guid postId, int limit = 0, int offset = 0)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Getting likers for PostId: {PostId}", postId);
        }

        var post = await unitOfWork.Posts.GetByIdAsync(postId);
        if (post is null)
        {
            throw new ResourceNotFoundException("post", postId);
        }

        return await unitOfWork.Likes.GetLikers(postId, limit, offset);
    }
}
