using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using BCrypt.Net;

namespace Application.Services;

/// <summary>
/// Servicio para la gestión de usuarios.
/// </summary>
public class UserService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<UserService> logger) : IUserService
{

    public async Task<UserDto> Create(CreateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando crear usuario con email: {Email}", model.Email);
        }

        if (await unitOfWork.Users.ExistsByEmailAsync(model.Email))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Intento de registro con email duplicado: {Email}", model.Email);
            }
            throw new AlreadyExistsException("usuario", "email", model.Email);
        }

        var defaultRoleId = await unitOfWork.Roles.GetRoleIdByNameAsync(RoleConstants.DefaultRole);

        if (!defaultRoleId.HasValue)
        {
            logger.LogError("No se encontró el rol por defecto: {Role}", RoleConstants.DefaultRole);
            throw new InvalidOperationException("No se pudo asignar el rol por defecto al usuario");
        }

        var entity = new User
        {
            UserId = Guid.NewGuid(),
            Nickname = model.Nickname,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(entity);

        // Asignar el rol por defecto
        AssignRoleToUser(entity.UserId, defaultRoleId.Value);

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", entity.UserId);
        }

        await emailService.SendWelcomeEmailAsync(entity.Email, entity.Nickname);

        return MapToDto(entity);
    }

    public async Task<UserDto> UpdateProfile(Guid userId, UpdateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar el perfil del usuario con ID: {UserId}", userId);
        }

        var existing = await unitOfWork.Users.GetByIdAsync(userId);

        if (existing is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para actualizar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var normalizedNickname = NormalizeOptionalProfileField(model.Nickname, nameof(model.Nickname));
        var normalizedEmail = NormalizeOptionalProfileField(model.Email, nameof(model.Email));
        var normalizedBiography = NormalizeBiography(model.Biography);

        if (normalizedEmail is not null
            && !string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)
            && await unitOfWork.Users.ExistsByEmailAsync(normalizedEmail))
        {
            throw new AlreadyExistsException("usuario", "email", normalizedEmail);
        }

        if (normalizedNickname is not null)
        {
            existing.Nickname = normalizedNickname;
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

    public async Task<GenericResponse<List<UserDto>>> Get(int limit, int offset, string? nickname = null, string? email = null)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Obteniendo lista de usuarios | Limit: {Limit}, Offset: {Offset}, Nombre: {Nickname}, Email: {Email}",
                limit, offset, nickname, email);
        }

        var users = await unitOfWork.Users.GetAllAsync(limit, offset, nickname, email);
        var dtos = users.Select(MapToDto).ToList();
        return new GenericResponse<List<UserDto>> { Data = dtos };
    }

    public async Task<UserDto> Get(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Buscando usuario con ID: {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);

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

        var user = await unitOfWork.Users.GetByIdAsync(userId);

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

        await emailService.SendPasswordChangedNotificationAsync(user.Email, user.Nickname);
    }

    public async Task Delete(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando soft-delete usuario con ID: {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);

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

        var user = await unitOfWork.Users.GetByIdAsync(userId);

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

    private void AssignRoleToUser(Guid userId, Guid roleId)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTimeHelper.UtcNow()
        };
        unitOfWork.Create(userRole);
    }

    private static UserDto MapToDto(User entity) => new()
    {
        UserId = entity.UserId,
        Nickname = entity.Nickname,
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

}
