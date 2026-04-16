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
using Shared;

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

        if (unitOfWork.userRepository.ExistsByEmail(model.Email))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Intento de registro con email duplicado: {Email}", model.Email);
            }
            throw new AlreadyExistsException("usuario", "email", model.Email);
        }

        // Obtener el RoleId del rol por defecto (User)
        var defaultRoleId = unitOfWork.roleRepository.GetRoleIdByName(RoleConstants.DefaultRole);

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
            PasswordHash = model.Password,
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        var created = unitOfWork.userRepository.Create(entity);

        // Asignar el rol por defecto en la tabla intermedia
        await AssignRoleToUser(created.UserId, defaultRoleId.Value);

        // Guardar todos los cambios
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", created.UserId);
        }

        // Enviar email de bienvenida
        await emailService.SendWelcomeEmailAsync(created.Email, created.FullName);

        return MapToDto(created);
    }

    public async Task<UserDto> Update(Guid userId, UpdateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar usuario con ID: {UserId}", userId);
        }

        var existing = unitOfWork.userRepository.GetById(userId);

        if (existing is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para actualizar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var updated = new User
        {
            UserId = existing.UserId,
            FullName = model.FullName ?? existing.FullName,
            Email = model.Email ?? existing.Email,
            IsActive = existing.IsActive,
            PasswordHash = existing.PasswordHash,
            CreatedAt = existing.CreatedAt
        };

        var result = unitOfWork.userRepository.Update(userId, updated);

        if (result is null)
        {
            logger.LogError("Error al actualizar usuario con ID: {UserId}", userId);
            throw new InvalidOperationException(ErrorConstants.INTERNAL_SERVER_ERROR);
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario actualizado exitosamente con ID: {UserId}", result.UserId);
        }
        return MapToDto(result);
    }

    public GenericResponse<List<UserDto>> Get(int limit, int offset, string? fullName = null, string? email = null)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Obteniendo lista de usuarios | Limit: {Limit}, Offset: {Offset}, Nombre: {FullName}, Email: {Email}",
                limit, offset, fullName, email);
        }

        var users = unitOfWork.userRepository.GetAll(limit, offset, fullName, email);
        var dtos = users.Select(MapToDto).ToList();
        return new GenericResponse<List<UserDto>> { Data = dtos };
    }

    public UserDto Get(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Buscando usuario con ID: {UserId}", userId);
        }

        var user = unitOfWork.userRepository.GetById(userId);

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

        var user = unitOfWork.userRepository.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para cambio de contraseña: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var result = unitOfWork.userRepository.ChangePassword(userId, model.NewPassword);

        if (!result)
        {
            logger.LogError("Error al cambiar contraseña del usuario con ID: {UserId}", userId);
            throw new InvalidOperationException("No se pudo cambiar la contraseña");
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Contraseña cambiada exitosamente para usuario con ID: {UserId}", userId);
        }

        // Enviar notificación de cambio de contraseña
        await emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);
    }

    public async Task Delete(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando eliminar usuario con ID: {UserId}", userId);
        }

        var user = unitOfWork.userRepository.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para eliminar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var result = unitOfWork.userRepository.Delete(userId);

        if (!result)
        {
            logger.LogError("Error al eliminar usuario con ID: {UserId}", userId);
            throw new InvalidOperationException("No se pudo eliminar el usuario");
        }

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario eliminado exitosamente con ID: {UserId}", userId);
        }
    }

    private async Task AssignRoleToUser(Guid userId, Guid roleId)
    {
        // TODO: Este método debería usar un repositorio específico para UserRoles
        // Por ahora, dejamos esta lógica manual pero en una implementación real
        // debería estar en Infrastructure.Persistence.Repositories.UserRoleRepository

        // Nota: Para implementar esto correctamente, necesitaríamos:
        // 1. Crear IUserRoleRepository con método Create
        // 2. Agregarlo a IUnitOfWork
        // 3. Implementar UserRoleRepository

        // Como solución temporal, este método queda vacío y la asignación de roles
        // se manejará en otra migración del patrón UoW
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
