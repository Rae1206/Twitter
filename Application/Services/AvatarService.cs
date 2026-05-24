using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Media;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Twitter.Domain.Exceptions;

namespace Application.Services;

public class AvatarService(
    IUnitOfWork unitOfWork,
    IMediaStorageService mediaStorageService,
    IConfiguration configuration,
    ILogger<AvatarService> logger) : IAvatarService
{
    private const string AvatarFolder = "avatars";

    public async Task<UserDto> UploadProfilePhotoAsync(Guid userId, UploadMediaRequest request)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando subir foto de perfil para usuario con ID: {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        ValidateAvatarFile(request);

        var previousStoragePath = user.ProfilePhotoStoragePath;
        var fileName = Path.GetFileName(request.FileName);
        var storagePath = await mediaStorageService.SaveAsync(request.FileStream, fileName, AvatarFolder);
        var publicUrl = await ResolveProfilePhotoUrlAsync(userId, storagePath);

        user.ProfilePhotoFileName = fileName;
        user.ProfilePhotoStoragePath = storagePath;
        user.ProfilePhotoUrl = publicUrl;

        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(previousStoragePath)
            && !string.Equals(previousStoragePath, storagePath, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await mediaStorageService.DeleteAsync(previousStoragePath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo eliminar la foto de perfil previa del usuario {UserId}", userId);
            }
        }

        return MapToDto(user);
    }

    public async Task<UserProfilePhotoDto> GetProfilePhotoAsync(Guid userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        return new UserProfilePhotoDto
        {
            FileName = user.ProfilePhotoFileName,
            StoragePath = user.ProfilePhotoStoragePath,
            Url = user.ProfilePhotoUrl
        };
    }

    private static void ValidateAvatarFile(UploadMediaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ValidationException("La foto de perfil debe incluir un nombre de archivo válido");
        }

        if (string.IsNullOrWhiteSpace(request.ContentType))
        {
            throw new ValidationException("La foto de perfil debe incluir un tipo MIME válido");
        }

        var fileName = Path.GetFileName(request.FileName);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = request.ContentType.ToLowerInvariant();
        var maxSizeBytes = MediaConstants.MaxImageSizeMb * 1024 * 1024;

        if (!MediaConstants.AllowedImageExtensions.Contains(extension))
        {
            throw new ValidationException($"Extensión no permitida para foto de perfil: {extension}");
        }

        if (!MediaConstants.AllowedImageMimeTypes.Contains(contentType))
        {
            throw new ValidationException($"Tipo MIME no permitido para foto de perfil: {request.ContentType}");
        }

        if (request.Length <= 0)
        {
            throw new ValidationException("La foto de perfil está vacía");
        }

        if (request.Length > maxSizeBytes)
        {
            throw new ValidationException($"La foto de perfil excede el tamaño máximo permitido de {MediaConstants.MaxImageSizeMb} MB");
        }
    }

    private async Task<string> ResolveProfilePhotoUrlAsync(Guid userId, string storagePath)
    {
        var storageProvider = configuration["Storage:Provider"]?.ToLowerInvariant() ?? "local";

        if (storageProvider == "digitalocean")
        {
            return await mediaStorageService.GetPublicUrlAsync(storagePath, userId);
        }

        return $"/api/user/{userId}/avatar";
    }

    private static UserDto MapToDto(User entity) => new()
    {
        UserId = entity.UserId,
        FullName = entity.FullName,
        Email = entity.Email,
        Biography = entity.Biography,
        ProfilePhotoUrl = entity.ProfilePhotoUrl,
        ProfilePhotoFileName = entity.ProfilePhotoFileName,
        IsActive = entity.IsActive,
        IsSuspended = entity.IsSuspended,
        IsShadowBanned = entity.IsShadowBanned,
        DeletedAt = entity.DeletedAt,
        CreatedAt = entity.CreatedAt,
        Roles = entity.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
    };
}
