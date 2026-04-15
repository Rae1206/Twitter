using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Domain.Context;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ILogger<UserService> logger) : IUserService
{
    public UserDto Create(CreateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando crear usuario con email: {Email}", model.Email);
        }

        if (userRepository.ExistsByEmail(model.Email))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Intento de registro con email duplicado: {Email}", model.Email);
            }
            throw new AlreadyExistsException("usuario", "email", model.Email);
        }

        // Obtener el RoleId del rol por defecto (User)
        var defaultRoleId = roleRepository.GetRoleIdByName(RoleConstants.DefaultRole);
        
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

        var created = userRepository.Create(entity);

        // Asignar el rol por defecto en la tabla intermedia
        AssignRoleToUser(created.UserId, defaultRoleId.Value);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", created.UserId);
        }
        return MapToDto(created);
    }

    public UserDto Update(Guid userId, UpdateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar usuario con ID: {UserId}", userId);
        }

        var existing = userRepository.GetById(userId);

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

        var result = userRepository.Update(userId, updated);

        if (result is null)
        {
            logger.LogError("Error al actualizar usuario con ID: {UserId}", userId);
            throw new InvalidOperationException(ErrorConstants.INTERNAL_SERVER_ERROR);
        }

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

        var users = userRepository.GetAll(limit, offset, fullName, email);
        var dtos = users.Select(MapToDto).ToList();
        return new GenericResponse<List<UserDto>> { Data = dtos };
    }

    public UserDto Get(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Buscando usuario con ID: {UserId}", userId);
        }

        var user = userRepository.GetById(userId);

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

    public void ChangePassword(Guid userId, ChangePasswordUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando cambiar contraseña del usuario con ID: {UserId}", userId);
        }

        var user = userRepository.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para cambio de contraseña: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var result = userRepository.ChangePassword(userId, model.NewPassword);

        if (!result)
        {
            logger.LogError("Error al cambiar contraseña del usuario con ID: {UserId}", userId);
            throw new InvalidOperationException("No se pudo cambiar la contraseña");
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Contraseña cambiada exitosamente para usuario con ID: {UserId}", userId);
        }
    }

    public void Delete(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando eliminar usuario con ID: {UserId}", userId);
        }

        var user = userRepository.GetById(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para eliminar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var result = userRepository.Delete(userId);

        if (!result)
        {
            logger.LogError("Error al eliminar usuario con ID: {UserId}", userId);
            throw new InvalidOperationException("No se pudo eliminar el usuario");
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario eliminado exitosamente con ID: {UserId}", userId);
        }
    }

    private void AssignRoleToUser(Guid userId, Guid roleId)
    {
        using var context = new TwitterDbContext();
        var userRole = new UserRole
        {
            UserRoleId = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTimeHelper.UtcNow()
        };
        context.UserRoles.Add(userRole);
        context.SaveChanges();
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
