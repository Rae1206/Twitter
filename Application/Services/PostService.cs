using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Application.Models.Responses;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Database.SqlServer.Entities.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Twitter.Domain.Exceptions;
using Shared.Helpers;

namespace Application.Services;

/// <summary>
/// Servicio para la gestión de posts.
/// </summary>
/// <summary>
/// Servicio encargado de la creación, modificación, obtención y eliminación de posts (publicaciones), incluyendo soporte para posts efímeros y archivos multimedia adjuntos.
/// </summary>
public class PostService(
    IUnitOfWork unitOfWork,
    ILogger<PostService> logger) : IPostService
{
    /// <summary>
    /// Crea de forma asíncrona una nueva publicación para un usuario.
    /// Soporta la creación de posts efímeros calculando la fecha de vencimiento si se especifica la duración.
    /// Asocia los archivos multimedia adjuntos indicados en el modelo de solicitud.
    /// </summary>
    /// <param name="currentUserId">Identificador único del usuario autor.</param>
    /// <param name="model">Modelo de solicitud con el contenido, archivos multimedia y duración del post efímero.</param>
    /// <returns>La representación en formato DTO <see cref="PostDto"/> del post creado.</returns>
    public async Task<PostDto> Create(Guid currentUserId, CreatePostRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando crear post para usuario: {UserId}", currentUserId);
        }

        DateTime? expiresAt = null;
        if (model.DurationMinutes.HasValue)
        {
            // El [Range] en el DTO ya valida los límites, pero defendemos doble por si se llama el service desde otro lugar.
            if (model.DurationMinutes.Value < PostConstants.MinEphemeralMinutes
                || model.DurationMinutes.Value > PostConstants.MaxEphemeralMinutes)
            {
                throw new ValidationException(
                    $"La duración del post efímero debe estar entre {PostConstants.MinEphemeralMinutes} y {PostConstants.MaxEphemeralMinutes} minutos");
            }

            expiresAt = DateTimeHelper.UtcNow().AddMinutes(model.DurationMinutes.Value);
        }

        var entity = new Post
        {
            PostId = Guid.NewGuid(),
            UserId = currentUserId,
            Content = model.Content,
            IsPublished = model.IsPublished ?? false,
            CreatedAt = DateTimeHelper.UtcNow(),
            ExpiresAt = expiresAt
        };

        unitOfWork.Create(entity);

        if (model.MediaIds is { Count: > 0 })
        {
            await AssociateMediaAsync(entity.PostId, currentUserId, model.MediaIds);
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Post creado exitosamente con ID: {PostId} (ExpiresAt: {ExpiresAt})", entity.PostId, entity.ExpiresAt);
        }
        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// Modifica de forma asíncrona los campos de una publicación existente.
    /// Valida que el post exista y que el usuario solicitante sea el propietario legítimo.
    /// Permite actualizar tanto el contenido de texto como la colección de archivos multimedia asociados.
    /// </summary>
    /// <param name="postId">Identificador único del post a actualizar.</param>
    /// <param name="currentUserId">Identificador único del usuario que realiza la solicitud.</param>
    /// <param name="model">Modelo de solicitud con los campos a actualizar.</param>
    /// <returns>La representación en formato DTO <see cref="PostDto"/> del post modificado.</returns>
    public async Task<PostDto> Update(Guid postId, Guid currentUserId, UpdatePostRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar post con ID: {PostId}", postId);
        }

        var existing = await unitOfWork.Posts.GetByIdAsync(postId);

        if (existing is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Post no encontrado para actualizar: {PostId}", postId);
            }
            throw new ResourceNotFoundException("post", postId);
        }

        EnsureOwnership(existing, currentUserId, "actualizar");

        existing.Content = model.Content ?? existing.Content;

        unitOfWork.Update(existing);

        if (model.MediaIds is not null)
        {
            await ReplaceMediaAsync(postId, currentUserId, model.MediaIds);
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Post actualizado exitosamente con ID: {PostId}", postId);
        }
        return await MapToDtoAsync(existing);
    }

    /// <summary>
    /// Obtiene una lista paginada de publicaciones representadas en formato DTO, permitiendo filtros opcionales de autor y estado.
    /// </summary>
    /// <param name="limit">Cantidad máxima de posts a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="userId">Identificador opcional del autor para filtrar las publicaciones.</param>
    /// <param name="isPublished">Filtro opcional para obtener únicamente borradores o publicados.</param>
    /// <returns>Una respuesta genérica que contiene el listado de <see cref="PostDto"/>.</returns>
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

    /// <summary>
    /// Obtiene de forma asíncrona los detalles de una publicación individual específica en formato DTO.
    /// Lanza una excepción si la publicación no existe.
    /// </summary>
    /// <param name="postId">Identificador único del post a recuperar.</param>
    /// <returns>La representación DTO <see cref="PostDto"/> del post solicitado.</returns>
    public async Task<PostDto> Get(Guid postId)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Buscando post con ID: {PostId}", postId);
        }

        var query = unitOfWork.Posts.GetQueryable().Where(p => p.PostId == postId);
        var dto = await ProjectPostToDto(query).FirstOrDefaultAsync();

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

    /// <summary>
    /// Cambia de forma asíncrona el estado de publicación (borrador/publicado) de un post determinado.
    /// Valida que el post exista y que el usuario solicitante sea el propietario.
    /// </summary>
    /// <param name="postId">Identificador único del post.</param>
    /// <param name="currentUserId">Identificador del usuario que realiza la solicitud.</param>
    /// <param name="model">Modelo conteniendo el nuevo estado de publicación.</param>
    public async Task ChangeStatus(Guid postId, Guid currentUserId, ChangePostStatusRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando cambiar estado del post con ID: {PostId}", postId);
        }

        var existing = await unitOfWork.Posts.GetByIdAsync(postId);
        
        if (existing is null)
        {
            logger.LogError("Error al cambiar estado del post con ID: {PostId}", postId);
            throw new ResourceNotFoundException("post", postId);
        }

        EnsureOwnership(existing, currentUserId, "cambiar el estado de");

        existing.IsPublished = model.IsPublished;
        unitOfWork.Update(existing);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Estado del post cambiado exitosamente con ID: {PostId}", postId);
        }
    }

    /// <summary>
    /// Elimina de forma asíncrona una publicación del sistema aplicando una eliminación lógica (DeletedAt).
    /// Valida que el post exista y pertenezca al usuario actual. También marca como eliminados de forma lógica todos los archivos multimedia adjuntos.
    /// </summary>
    /// <param name="postId">Identificador único de la publicación a eliminar.</param>
    /// <param name="currentUserId">Identificador del usuario que realiza la acción.</param>
    public async Task Delete(Guid postId, Guid currentUserId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando eliminar post con ID: {PostId}", postId);
        }

        var post = await unitOfWork.Posts.GetByIdAsync(postId);

        if (post is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Post no encontrado para eliminar: {PostId}", postId);
            }
            throw new ResourceNotFoundException("post", postId);
        }

        EnsureOwnership(post, currentUserId, "eliminar");

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

    /// <summary>
    /// Valida de manera estricta que una publicación pertenezca al usuario solicitante.
    /// Lanza una excepción <see cref="ForbiddenException"/> en caso de no ser el autor.
    /// </summary>
    /// <param name="post">La entidad del post a comprobar.</param>
    /// <param name="currentUserId">Identificador del usuario que ejecuta la acción.</param>
    /// <param name="action">Nombre textual descriptivo de la acción para efectos de logueo.</param>
    private void EnsureOwnership(Post post, Guid currentUserId, string action)
    {
        if (post.UserId == currentUserId)
        {
            return;
        }

        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Intento no autorizado de {Action} post {PostId} por usuario {CurrentUserId} (dueño: {OwnerId})",
                action, post.PostId, currentUserId, post.UserId);
        }

        throw new ForbiddenException($"No tiene permisos para {action} esta publicación");
    }

    /// <summary>
    /// Vincula de forma asíncrona un conjunto de archivos multimedia a una publicación específica.
    /// Aplica validaciones de límites de almacenamiento, tipos de archivo permitidos y autoría de los recursos multimedia.
    /// </summary>
    /// <param name="postId">Identificador único de la publicación destino.</param>
    /// <param name="userId">Identificador del usuario autor.</param>
    /// <param name="mediaIds">Conjunto de identificadores de los archivos multimedia.</param>
    private async Task AssociateMediaAsync(Guid postId, Guid userId, List<Guid> mediaIds)
    {
        if (mediaIds.Count > MediaConstants.MaxMediaPerPost)
        {
            throw new ValidationException($"Máximo {MediaConstants.MaxMediaPerPost} archivos permitidos por publicación");
        }

        int audioCount = 0;
        foreach (var mediaId in mediaIds)
        {
            var media = await unitOfWork.PostMedias.GetByIdAsync(mediaId);
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

    /// <summary>
    /// Reemplaza de forma asíncrona los archivos multimedia asociados a una publicación, liberando los vínculos anteriores y asociando los nuevos recursos.
    /// </summary>
    /// <param name="postId">Identificador único de la publicación.</param>
    /// <param name="userId">Identificador del usuario autor.</param>
    /// <param name="mediaIds">Listado con las nuevas identidades multimedia a vincular.</param>
    private async Task ReplaceMediaAsync(Guid postId, Guid userId, List<Guid> mediaIds)
    {
        var existingMedia = await unitOfWork.PostMedias.GetByPostIdAsync(postId);
        foreach (var media in existingMedia)
        {
            media.PostId = null;
            unitOfWork.Update(media);
        }

        if (mediaIds.Count > 0)
        {
            await AssociateMediaAsync(postId, userId, mediaIds);
        }
    }

    /// <summary>
    /// Proyecta una consulta sobre entidades <see cref="Post"/> de Entity Framework directamente en objetos DTO <see cref="PostDto"/> de forma optimizada.
    /// </summary>
    /// <param name="query">Consulta IQueryable sobre la entidad Post.</param>
    /// <returns>La consulta estructurada proyectada en PostDto.</returns>
    private IQueryable<PostDto> ProjectPostToDto(IQueryable<Post> query)
    {
        return query.Select(p => new PostDto
        {
            PostId = p.PostId,
            UserId = p.UserId,
            UserNickname = p.User != null ? p.User.Nickname : string.Empty,
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
            CreatedAt = p.CreatedAt,
            ExpiresAt = p.ExpiresAt
        });
    }

    /// <summary>
    /// Mapea de forma asíncrona un objeto de entidad <see cref="Post"/> a su formato de respuesta DTO de forma segura en memoria si es necesario.
    /// </summary>
    /// <param name="entity">La entidad del post a convertir.</param>
    /// <returns>El DTO <see cref="PostDto"/> correspondiente.</returns>
    private async Task<PostDto> MapToDtoAsync(Post entity)
    {
        var query = unitOfWork.Posts.GetQueryable().Where(p => p.PostId == entity.PostId);
        var dto = ProjectPostToDto(query).FirstOrDefault();

        if (dto is null)
        {
            var media = await unitOfWork.PostMedias.GetByPostIdAsync(entity.PostId);
            var user = entity.User ?? await unitOfWork.Users.GetByIdAsync(entity.UserId);
            dto = new PostDto
            {
                PostId = entity.PostId,
                UserId = entity.UserId,
                UserNickname = user?.Nickname ?? string.Empty,
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
                CreatedAt = entity.CreatedAt,
                ExpiresAt = entity.ExpiresAt
            };
        }

        return dto;
    }
}
