using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Media;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;
using BCrypt.Net;
using Twitter.Domain.Interfaces.Services;

namespace Application.Services;

/// <summary>
/// Servicio para la gestión de usuarios.
/// </summary>
public class UserService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IMediaStorageService mediaStorageService,
    IConfiguration configuration,
    ILogger<UserService> logger) : IUserService
{
    private const string AvatarFolder = "avatars";

    public async Task<UserDto> Create(CreateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando crear usuario con email: {Email}", model.Email);
        }

        if (unitOfWork.Users.ExistsByEmail(model.Email))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Intento de registro con email duplicado: {Email}", model.Email);
            }
            throw new AlreadyExistsException("usuario", "email", model.Email);
        }

        var defaultRoleId = unitOfWork.Roles.GetRoleIdByName(RoleConstants.DefaultRole);

        if (!defaultRoleId.HasValue)
        {
            logger.LogError("No se encontró el rol por defecto: {Role}", RoleConstants.DefaultRole);
            throw new InvalidOperationException("No se pudo asignar el rol por defecto al usuario");
        }

        var entity = new User
        {
            UserId = Guid.NewGuid(),
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(entity);

        // Asignar el rol por defecto
        await AssignRoleToUser(entity.UserId, defaultRoleId.Value);

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", entity.UserId);
        }

        await emailService.SendWelcomeEmailAsync(entity.Email, entity.FullName);

        return MapToDto(entity);
    }

    public async Task<UserDto> UpdateProfile(Guid userId, UpdateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar el perfil del usuario con ID: {UserId}", userId);
        }

        var existing = unitOfWork.Users.GetById(userId);

        if (existing is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para actualizar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var normalizedFullName = NormalizeOptionalProfileField(model.FullName, nameof(model.FullName));
        var normalizedEmail = NormalizeOptionalProfileField(model.Email, nameof(model.Email));
        var normalizedBiography = NormalizeBiography(model.Biography);

        if (normalizedEmail is not null
            && !string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)
            && unitOfWork.Users.ExistsByEmail(normalizedEmail))
        {
            throw new AlreadyExistsException("usuario", "email", normalizedEmail);
        }

        if (normalizedFullName is not null)
        {
            existing.FullName = normalizedFullName;
        }

        if (normalizedEmail is not null)
        {
            existing.Email = normalizedEmail;
        }

        if (model.Biography is not null)
        {
            existing.Biography = normalizedBiography;
        }

        unitOfWork.Update(existing);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario actualizado exitosamente con ID: {UserId}", userId);
        }
        return MapToDto(existing);
    }

    public GenericResponse<List<UserDto>> Get(int limit, int offset, string? fullName = null, string? email = null)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Obteniendo lista de usuarios | Limit: {Limit}, Offset: {Offset}, Nombre: {FullName}, Email: {Email}",
                limit, offset, fullName, email);
        }

        var users = unitOfWork.Users.GetAll(limit, offset, fullName, email);
        var dtos = users.Select(MapToDto).ToList();
        return new GenericResponse<List<UserDto>> { Data = dtos };
    }

    public UserDto Get(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Buscando usuario con ID: {UserId}", userId);
        }

        var user = unitOfWork.Users.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado con ID: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        return MapToDto(user);
    }

    public async Task ChangePassword(Guid userId, ChangePasswordUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando cambiar contraseña del usuario con ID: {UserId}", userId);
        }

        var user = unitOfWork.Users.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para cambio de contraseña: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Contraseña cambiada exitosamente para usuario con ID: {UserId}", userId);
        }

        await emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);
    }

    public async Task<UserDto> UploadProfilePhoto(Guid userId, UploadMediaRequest request)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando subir foto de perfil para usuario con ID: {UserId}", userId);
        }

        var user = unitOfWork.Users.GetById(userId);

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

    public UserProfilePhotoDto GetProfilePhoto(Guid userId)
    {
        var user = unitOfWork.Users.GetById(userId);

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

    public async Task Delete(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando soft-delete usuario con ID: {UserId}", userId);
        }

        var user = unitOfWork.Users.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para eliminar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.DeletedAt = DateTimeHelper.UtcNow();
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario soft-deleted exitosamente con ID: {UserId}", userId);
        }
    }

    public async Task Restore(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando restaurar usuario con ID: {UserId}", userId);
        }

        var user = unitOfWork.Users.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para restaurar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.DeletedAt = null;
        user.DeletedByAdminId = null;
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario restaurado exitosamente con ID: {UserId}", userId);
        }
    }

    private async Task AssignRoleToUser(Guid userId, Guid roleId)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTimeHelper.UtcNow()
        };
        unitOfWork.Create(userRole);
        await Task.CompletedTask;
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

    private static string? NormalizeOptionalProfileField(string? value, string fieldName)
    {
        if (value is null)
        {
            return null;
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length == 0)
        {
            throw new ValidationException($"El campo {fieldName} no puede estar vacío");
        }

        return normalizedValue;
    }

    private static string? NormalizeBiography(string? biography)
    {
        if (biography is null)
        {
            return null;
        }

        var normalizedBiography = biography.Trim();
        return normalizedBiography.Length == 0 ? null : normalizedBiography;
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
}
