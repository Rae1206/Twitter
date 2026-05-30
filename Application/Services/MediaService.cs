using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Media;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Database.SqlServer.Entities.Enums;
using Twitter.Domain.Interfaces.Services;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio encargado de procesar la subida, obtención, eliminación y limpieza de archivos multimedia.
/// </summary>
public class MediaService(
    IUnitOfWork unitOfWork,
    IMediaStorageService storageService,
    ILogger<MediaService> logger) : IMediaService
{
    /// <summary>
    /// Procesa la subida física de un archivo multimedia, determina su tipo, lo valida y registra en base de datos.
    /// </summary>
    /// <param name="request">Los datos de transferencia del archivo y su flujo de datos.</param>
    /// <param name="userId">Identificador único del usuario que sube el archivo.</param>
    /// <returns>Los metadatos del archivo subido en forma de DTO.</returns>
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

    /// <summary>
    /// Obtiene una entidad multimedia por su identificador único.
    /// </summary>
    /// <param name="mediaId">Identificador único del archivo multimedia.</param>
    /// <returns>La entidad de base de datos del archivo multimedia o null.</returns>
    public async Task<PostMedia?> GetByIdAsync(Guid mediaId)
    {
        return await unitOfWork.PostMedias.GetByIdAsync(mediaId);
    }

    /// <summary>
    /// Obtiene todos los archivos multimedia asociados a una publicación específica.
    /// </summary>
    /// <param name="postId">Identificador único de la publicación.</param>
    /// <returns>La lista de archivos multimedia de la publicación.</returns>
    public async Task<List<PostMedia>> GetByPostIdAsync(Guid postId)
    {
        return await unitOfWork.PostMedias.GetByPostIdAsync(postId);
    }

    /// <summary>
    /// Elimina un archivo multimedia de forma física en el disco/nube y elimina su registro en base de datos.
    /// </summary>
    /// <param name="mediaId">Identificador único del archivo multimedia a eliminar.</param>
    /// <param name="userId">Identificador del usuario que solicita la eliminación para comprobar permisos.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task DeleteAsync(Guid mediaId, Guid userId)
    {
        var media = await unitOfWork.PostMedias.GetByIdAsync(mediaId);
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

    /// <summary>
    /// Identifica y elimina del sistema de archivos y base de datos aquellos archivos multimedia huérfanos con una antigüedad determinada.
    /// </summary>
    /// <param name="maxAge">Antigüedad máxima que determina si un archivo huérfano debe ser limpiado.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
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

    /// <summary>
    /// Determina el tipo de medio (MediaType: Image, Video, Gif, Audio) a partir del nombre y tipo MIME provistos.
    /// </summary>
    /// <param name="fileName">Nombre del archivo físico.</param>
    /// <param name="contentType">Tipo MIME o content-type provisto.</param>
    /// <returns>El tipo de medio identificado.</returns>
    private static MediaType DetermineMediaType(string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var normalizedContentType = NormalizeMimeType(contentType);

        if (MediaConstants.AllowedImageExtensions.Contains(ext) || MediaConstants.AllowedImageMimeTypes.Contains(normalizedContentType))
            return MediaType.Image;

        if (MediaConstants.AllowedGifExtensions.Contains(ext) || MediaConstants.AllowedGifMimeTypes.Contains(normalizedContentType))
            return MediaType.Gif;

        if (MediaConstants.AllowedAudioExtensions.Contains(ext) || MediaConstants.AllowedAudioMimeTypes.Contains(normalizedContentType))
            return MediaType.Audio;

        if (MediaConstants.AllowedVideoExtensions.Contains(ext) || MediaConstants.AllowedVideoMimeTypes.Contains(normalizedContentType))
            return MediaType.Video;

        throw new MediaValidationException($"Tipo de archivo no soportado: {ext}");
    }

    /// <summary>
    /// Valida que el archivo cumpla con las extensiones, tipos MIME y límites de tamaño según el tipo de medio determinado.
    /// </summary>
    /// <param name="request">Los datos de la solicitud de subida.</param>
    /// <param name="mediaType">Tipo de medio determinado para el archivo.</param>
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

        var contentType = NormalizeMimeType(request.ContentType);

        if (!allowedMimes.Contains(contentType))
        {
            throw new MediaValidationException($"Tipo MIME no permitido: {request.ContentType}");
        }

        if (request.Length > maxSizeBytes)
        {
            throw new MediaValidationException($"El archivo excede el tamaño máximo permitido de {maxSizeBytes / 1024 / 1024} MB");
        }
    }

    /// <summary>
    /// Normaliza el tipo MIME eliminando cualquier parámetro adicional como codificaciones o códecs de audio/video.
    /// </summary>
    /// <param name="contentType">Tipo de contenido original.</param>
    /// <returns>El tipo MIME normalizado sin parámetros.</returns>
    private static string NormalizeMimeType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return string.Empty;
        }

        // Stripear parámetros de codec/charset (ej: "audio/webm;codecs=opus" -> "audio/webm")
        var separatorIndex = contentType.IndexOf(';');
        var baseType = separatorIndex > 0 ? contentType.Substring(0, separatorIndex) : contentType;
        return baseType.Trim().ToLowerInvariant();
    }
}
