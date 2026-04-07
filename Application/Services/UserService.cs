using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger) : IUserService
{
    public UserDto Create(CreateUserRequest model)
    {
        logger.LogInformation("Intentando crear usuario con email: {Email}", model.Email);

        if (userRepository.ExistsByEmail(model.Email))
        {
            logger.LogWarning("Intento de registro con email duplicado: {Email}", model.Email);
            throw new AlreadyExistsException("usuario", "email", model.Email);
        }

        var entity = new User
        {
            UserId = Guid.NewGuid(),
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = model.Password,
            IsActive = true,
            Role = RoleConstants.DefaultRole,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        var created = userRepository.Create(entity);
        logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", created.UserId);
        return MapToDto(created);
    }

    public UserDto Update(Guid userId, UpdateUserRequest model)
    {
        logger.LogInformation("Intentando actualizar usuario con ID: {UserId}", userId);

        var existing = userRepository.GetById(userId);

        if (existing is null)
        {
            logger.LogWarning("Usuario no encontrado para actualizar: {UserId}", userId);
            throw new ResourceNotFoundException("usuario", userId);
        }

        var updated = new User
        {
            UserId = existing.UserId,
            FullName = model.FullName ?? existing.FullName,
            Email = model.Email ?? existing.Email,
            IsActive = existing.IsActive,
            Role = existing.Role,
            PasswordHash = existing.PasswordHash,
            CreatedAt = existing.CreatedAt
        };

        var result = userRepository.Update(userId, updated);

        if (result is null)
        {
            logger.LogError("Error al actualizar usuario con ID: {UserId}", userId);
            throw new InvalidOperationException(ErrorConstants.INTERNAL_SERVER_ERROR);
        }

        logger.LogInformation("Usuario actualizado exitosamente con ID: {UserId}", result.UserId);
        return MapToDto(result);
    }

    public GenericResponse<List<UserDto>> Get(int limit, int offset, string? fullName = null, string? email = null)
    {
        logger.LogDebug("Obteniendo lista de usuarios | Limit: {Limit}, Offset: {Offset}, Nombre: {FullName}, Email: {Email}",
            limit, offset, fullName, email);

        var users = userRepository.GetAll(limit, offset, fullName, email);
        var dtos = users.Select(MapToDto).ToList();
        return new GenericResponse<List<UserDto>> { Data = dtos };
    }

    public UserDto Get(Guid userId)
    {
        logger.LogDebug("Buscando usuario con ID: {UserId}", userId);

        var user = userRepository.GetById(userId);

        if (user is null)
        {
            logger.LogWarning("Usuario no encontrado con ID: {UserId}", userId);
            throw new ResourceNotFoundException("usuario", userId);
        }

        return MapToDto(user);
    }

    public void ChangePassword(Guid userId, ChangePasswordUserRequest model)
    {
        logger.LogInformation("Intentando cambiar contraseña del usuario con ID: {UserId}", userId);

        var user = userRepository.GetById(userId);

        if (user is null)
        {
            logger.LogWarning("Usuario no encontrado para cambio de contraseña: {UserId}", userId);
            throw new ResourceNotFoundException("usuario", userId);
        }

        var result = userRepository.ChangePassword(userId, model.NewPassword);

        if (!result)
        {
            logger.LogError("Error al cambiar contraseña del usuario con ID: {UserId}", userId);
            throw new InvalidOperationException("No se pudo cambiar la contraseña");
        }

        logger.LogInformation("Contraseña cambiada exitosamente para usuario con ID: {UserId}", userId);
    }

    public void Delete(Guid userId)
    {
        logger.LogInformation("Intentando eliminar usuario con ID: {UserId}", userId);

        var user = userRepository.GetById(userId);

        if (user is null)
        {
            logger.LogWarning("Usuario no encontrado para eliminar: {UserId}", userId);
            throw new ResourceNotFoundException("usuario", userId);
        }

        var result = userRepository.Delete(userId);

        if (!result)
        {
            logger.LogError("Error al eliminar usuario con ID: {UserId}", userId);
            throw new InvalidOperationException("No se pudo eliminar el usuario");
        }

        logger.LogInformation("Usuario eliminado exitosamente con ID: {UserId}", userId);
    }

    private static UserDto MapToDto(User entity) => new()
    {
        UserId = entity.UserId,
        FullName = entity.FullName,
        Email = entity.Email,
        IsActive = entity.IsActive,
        Role = entity.Role,
        CreatedAt = entity.CreatedAt
    };
}
