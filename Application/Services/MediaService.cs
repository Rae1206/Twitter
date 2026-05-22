using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Media;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Database.SqlServer.Entities.Enums;
using Twitter.Domain.Interfaces.Services;

namespace Application.Services;

public class MediaService(
    IUnitOfWork unitOfWork,
    IMediaStorageService storageService,
    ILogger<MediaService> logger) : IMediaService
{
    public async Task<MediaUploadDto> UploadAsync(UploadMediaRequest request, Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Uploading media for user: {UserId}", userId);
        }

        var mediaType = DetermineMediaType(request.FileName, request.ContentType);
        ValidateFile(request, mediaType);

        var fileName = Path.GetFileName(request.FileName);
        var typeFolder = mediaType.ToString().ToLowerInvariant();

        var mediaId = Guid.NewGuid();
        var storagePath = await storageService.SaveAsync(request.FileStream, fileName, typeFolder);
        var publicUrl = await storageService.GetPublicUrlAsync(storagePath, mediaId);

        var media = new PostMedia
        {
            MediaId = mediaId,
            UserId = userId,
            MediaType = mediaType,
            FileName = fileName,
            StoragePath = storagePath,
            Url = publicUrl,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(media);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Media uploaded successfully: {MediaId}", media.MediaId);
        }

        return new MediaUploadDto
        {
            MediaId = media.MediaId,
            Url = media.Url,
            MediaType = media.MediaType
        };
    }

    public async Task<PostMedia?> GetByIdAsync(Guid mediaId)
    {
        return unitOfWork.PostMedias.GetById(mediaId);
    }

    public async Task<List<PostMedia>> GetByPostIdAsync(Guid postId)
    {
        return await unitOfWork.PostMedias.GetByPostIdAsync(postId);
    }

    public async Task DeleteAsync(Guid mediaId, Guid userId)
    {
        var media = unitOfWork.PostMedias.GetById(mediaId);
        if (media is null)
        {
            throw new ResourceNotFoundException("media", mediaId);
        }

        if (media.UserId != userId)
        {
            throw new ForbiddenException("No tiene permisos para eliminar este archivo");
        }

        await storageService.DeleteAsync(media.StoragePath);
        unitOfWork.Delete(media);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Media deleted: {MediaId}", mediaId);
        }
    }

    public async Task CleanupOrphansAsync(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow.Subtract(maxAge);
        var orphans = await unitOfWork.PostMedias.GetOrphansOlderThanAsync(cutoff);

        foreach (var orphan in orphans)
        {
            try
            {
                await storageService.DeleteAsync(orphan.StoragePath);
                unitOfWork.Delete(orphan);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete orphaned media: {MediaId}", orphan.MediaId);
            }
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Cleaned up {Count} orphaned media files", orphans.Count);
        }
    }

    private static MediaType DetermineMediaType(string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (MediaConstants.AllowedImageExtensions.Contains(ext) || MediaConstants.AllowedImageMimeTypes.Contains(contentType))
            return MediaType.Image;

        if (MediaConstants.AllowedGifExtensions.Contains(ext) || MediaConstants.AllowedGifMimeTypes.Contains(contentType))
            return MediaType.Gif;

        if (MediaConstants.AllowedAudioExtensions.Contains(ext) || MediaConstants.AllowedAudioMimeTypes.Contains(contentType))
            return MediaType.Audio;

        if (MediaConstants.AllowedVideoExtensions.Contains(ext) || MediaConstants.AllowedVideoMimeTypes.Contains(contentType))
            return MediaType.Video;

        throw new MediaValidationException($"Tipo de archivo no soportado: {ext}");
    }

    private static void ValidateFile(UploadMediaRequest request, MediaType mediaType)
    {
        var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
        long maxSizeBytes;
        string[] allowedExts;
        string[] allowedMimes;

        switch (mediaType)
        {
            case MediaType.Image:
                maxSizeBytes = MediaConstants.MaxImageSizeMb * 1024 * 1024;
                allowedExts = MediaConstants.AllowedImageExtensions;
                allowedMimes = MediaConstants.AllowedImageMimeTypes;
                break;
            case MediaType.Gif:
                maxSizeBytes = MediaConstants.MaxGifSizeMb * 1024 * 1024;
                allowedExts = MediaConstants.AllowedGifExtensions;
                allowedMimes = MediaConstants.AllowedGifMimeTypes;
                break;
            case MediaType.Audio:
                maxSizeBytes = MediaConstants.MaxAudioSizeMb * 1024 * 1024;
                allowedExts = MediaConstants.AllowedAudioExtensions;
                allowedMimes = MediaConstants.AllowedAudioMimeTypes;
                break;
            case MediaType.Video:
                maxSizeBytes = MediaConstants.MaxVideoSizeMb * 1024 * 1024;
                allowedExts = MediaConstants.AllowedVideoExtensions;
                allowedMimes = MediaConstants.AllowedVideoMimeTypes;
                break;
            default:
                throw new MediaValidationException("Tipo de media no válido");
        }

        if (!allowedExts.Contains(ext))
        {
            throw new MediaValidationException($"Extensión no permitida: {ext}");
        }

        if (!allowedMimes.Contains(request.ContentType.ToLowerInvariant()))
        {
            throw new MediaValidationException($"Tipo MIME no permitido: {request.ContentType}");
        }

        if (request.Length > maxSizeBytes)
        {
            throw new MediaValidationException($"El archivo excede el tamaño máximo permitido de {maxSizeBytes / 1024 / 1024} MB");
        }
    }
}
