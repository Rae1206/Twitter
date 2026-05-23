using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Database.SqlServer.Entities.Enums;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;

namespace Application.Services;

/// <summary>
/// Servicio para la gestión de posts.
/// </summary>
public class PostService(
    IUnitOfWork unitOfWork,
    ILogger<PostService> logger) : IPostService
{
    public async Task<PostDto> Create(CreatePostRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando crear post para usuario: {UserId}", model.UserId);
        }

        var entity = new Post
        {
            PostId = Guid.NewGuid(),
            UserId = model.UserId,
            Content = model.Content,
            IsPublished = model.IsPublished ?? false,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(entity);

        if (model.MediaIds is { Count: > 0 })
        {
            await AssociateMediaAsync(entity.PostId, model.UserId, model.MediaIds);
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Post creado exitosamente con ID: {PostId}", entity.PostId);
        }
        return await MapToDtoAsync(entity);
    }

    public async Task<PostDto> Update(Guid postId, UpdatePostRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar post con ID: {PostId}", postId);
        }

        var existing = unitOfWork.Posts.GetById(postId);

        if (existing is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Post no encontrado para actualizar: {PostId}", postId);
            }
            throw new ResourceNotFoundException("post", postId);
        }

        existing.Content = model.Content ?? existing.Content;
        if (model.UserId.HasValue)
            existing.UserId = model.UserId.Value;

        unitOfWork.Update(existing);

        if (model.MediaIds is not null)
        {
            await ReplaceMediaAsync(postId, model.UserId ?? existing.UserId, model.MediaIds);
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Post actualizado exitosamente con ID: {PostId}", postId);
        }
        return await MapToDtoAsync(existing);
    }

    public GenericResponse<List<PostDto>> Get(int limit, int offset, Guid? userId = null, bool? isPublished = null)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Obteniendo lista de posts | Limit: {Limit}, Offset: {Offset}, UserId: {UserId}, Publicado: {IsPublished}",
                limit, offset, userId, isPublished);
        }

        var query = unitOfWork.Posts.GetQueryable();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        var dtos = ProjectPostToDto(query)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();

        return new GenericResponse<List<PostDto>> { Data = dtos };
    }

    public PostDto Get(Guid postId)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Buscando post con ID: {PostId}", postId);
        }

        var query = unitOfWork.Posts.GetQueryable().Where(p => p.PostId == postId);
        var dto = ProjectPostToDto(query).FirstOrDefault();

        if (dto is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Post no encontrado con ID: {PostId}", postId);
            }
            throw new ResourceNotFoundException("post", postId);
        }

        return dto;
    }

    public async Task ChangeStatus(Guid postId, ChangePostStatusRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando cambiar estado del post con ID: {PostId}", postId);
        }

        var existing = unitOfWork.Posts.GetById(postId);
        
        if (existing is null)
        {
            logger.LogError("Error al cambiar estado del post con ID: {PostId}", postId);
            throw new ResourceNotFoundException("post", postId);
        }

        existing.IsPublished = model.IsPublished;
        unitOfWork.Update(existing);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Estado del post cambiado exitosamente con ID: {PostId}", postId);
        }
    }

    public async Task Delete(Guid postId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando eliminar post con ID: {PostId}", postId);
        }

        var post = unitOfWork.Posts.GetById(postId);

        if (post is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Post no encontrado para eliminar: {PostId}", postId);
            }
            throw new ResourceNotFoundException("post", postId);
        }

        post.DeletedAt = DateTimeHelper.UtcNow();
        unitOfWork.Update(post);

        var mediaList = await unitOfWork.PostMedias.GetByPostIdAsync(postId);
        foreach (var media in mediaList)
        {
            media.IsDeleted = true;
            unitOfWork.Update(media);
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Post eliminado exitosamente con ID: {PostId}", postId);
        }
    }

    private async Task AssociateMediaAsync(Guid postId, Guid userId, List<Guid> mediaIds)
    {
        if (mediaIds.Count > MediaConstants.MaxMediaPerPost)
        {
            throw new ValidationException($"Máximo {MediaConstants.MaxMediaPerPost} archivos permitidos por publicación");
        }

        int audioCount = 0;
        foreach (var mediaId in mediaIds)
        {
            var media = unitOfWork.PostMedias.GetById(mediaId);
            if (media is null)
            {
                throw new ResourceNotFoundException("media", mediaId);
            }

            if (media.UserId != userId)
            {
                throw new ForbiddenException("No tiene permisos para usar este archivo");
            }

            if (media.PostId is not null)
            {
                throw new ConflictException("El archivo ya está asociado a otra publicación");
            }

            if (media.MediaType == MediaType.Audio)
            {
                audioCount++;
            }

            media.PostId = postId;
            unitOfWork.Update(media);
        }

        if (audioCount > MediaConstants.MaxAudioPerPost)
        {
            throw new ValidationException($"Máximo {MediaConstants.MaxAudioPerPost} archivo de audio permitido por publicación");
        }
    }

    private async Task ReplaceMediaAsync(Guid postId, Guid userId, List<Guid> mediaIds)
    {
        // Unlink existing media
        var existingMedia = await unitOfWork.PostMedias.GetByPostIdAsync(postId);
        foreach (var media in existingMedia)
        {
            media.PostId = null;
            unitOfWork.Update(media);
        }

        // Link new media
        if (mediaIds.Count > 0)
        {
            await AssociateMediaAsync(postId, userId, mediaIds);
        }
    }

    private IQueryable<PostDto> ProjectPostToDto(IQueryable<Post> query)
    {
        return query.Select(p => new PostDto
        {
            PostId = p.PostId,
            UserId = p.UserId,
            UserFullName = p.User != null ? p.User.FullName : string.Empty,
            UserAvatar = null,
            Username = p.User != null ? p.User.Email : string.Empty,
            Content = p.Content,
            RepliedToPostId = p.RepliedToPostId,
            RetweetOfPostId = p.RetweetOfPostId,
            IsPublished = p.IsPublished,
            ReportCount = p.ReportCount,
            IsFlagged = p.IsFlagged,
            DeletedReason = p.DeletedReason,
            LikesCount = p.Likes.Count(),
            RetweetsCount = p.Retweets.Count(),
            RepliesCount = p.Replies.Count(),
            MediaUrls = p.PostMedias.Select(m => m.Url).ToList(),
            CreatedAt = p.CreatedAt
        });
    }

    private async Task<PostDto> MapToDtoAsync(Post entity)
    {
        var query = unitOfWork.Posts.GetQueryable().Where(p => p.PostId == entity.PostId);
        var dto = ProjectPostToDto(query).FirstOrDefault();

        if (dto is null)
        {
            var media = await unitOfWork.PostMedias.GetByPostIdAsync(entity.PostId);
            var user = entity.User ?? unitOfWork.Users.GetById(entity.UserId);
            dto = new PostDto
            {
                PostId = entity.PostId,
                UserId = entity.UserId,
                UserFullName = user?.FullName ?? string.Empty,
                UserAvatar = null,
                Username = user?.Email ?? string.Empty,
                Content = entity.Content,
                RepliedToPostId = entity.RepliedToPostId,
                RetweetOfPostId = entity.RetweetOfPostId,
                IsPublished = entity.IsPublished,
                ReportCount = entity.ReportCount,
                IsFlagged = entity.IsFlagged,
                DeletedReason = entity.DeletedReason,
                LikesCount = 0,
                RetweetsCount = 0,
                RepliesCount = 0,
                MediaUrls = media.Select(m => m.Url).ToList(),
                CreatedAt = entity.CreatedAt
            };
        }

        return dto;
    }
}
