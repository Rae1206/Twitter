using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Exceptions;
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

    public async Task<UserDto> Update(Guid userId, UpdateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar usuario con ID: {UserId}", userId);
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

        existing.FullName = model.FullName ?? existing.FullName;
        existing.Email = model.Email ?? existing.Email;

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

    public async Task Delete(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando eliminar usuario con ID: {UserId}", userId);
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

        unitOfWork.Delete(user);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario eliminado exitosamente con ID: {UserId}", userId);
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
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        Roles = entity.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
    };
}
