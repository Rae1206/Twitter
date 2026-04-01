using Application.Helpers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Shared.Helpers;

namespace Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public GenericResponse<UserDto> Create(CreateUserRequest model)
    {
        var user = new UserDto
        {
            UserId = Guid.NewGuid(),
            FullName = model.FullName,
            Email = model.Email,
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        var created = userRepository.Create(user);
        return ResponseHelper.Create(created);
    }

    public GenericResponse<UserDto> Update(Guid userId, UpdateUserRequest model)
    {
        var user = userRepository.GetById(userId);

        if (user is null)
        {
            return ResponseHelper.Create(new UserDto(), "Usuario no encontrado");
        }

        var updatedUser = new UserDto
        {
            UserId = user.UserId,
            FullName = model.FullName ?? user.FullName,
            Email = model.Email ?? user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        var result = userRepository.Update(userId, updatedUser);
        return result is null 
            ? ResponseHelper.Create(new UserDto(), "Error al actualizar") 
            : ResponseHelper.Create(result);
    }

    public GenericResponse<List<UserDto>> Get(int limit, int offset)
    {
        var users = userRepository.GetAll(limit, offset);
        return ResponseHelper.Create(users);
    }

    public GenericResponse<UserDto?> Get(Guid userId)
    {
        var user = userRepository.GetById(userId);
        return ResponseHelper.Create(user);
    }

    public GenericResponse<bool> ChangePassword(Guid userId, ChangePasswordUserRequest model)
    {
        var user = userRepository.GetById(userId);

        if (user is null)
        {
            return ResponseHelper.Create(false, "Usuario no encontrado");
        }

        var result = userRepository.ChangePassword(userId, model.NewPassword);
        return result 
            ? ResponseHelper.Create(true, "Contraseña actualizada correctamente")
            : ResponseHelper.Create(false, "Error al cambiar contraseña");
    }

    public GenericResponse<bool> Delete(Guid userId)
    {
        var user = userRepository.GetById(userId);

        if (user is null)
        {
            return ResponseHelper.Create(false);
        }

        var result = userRepository.Delete(userId);
        return result ? ResponseHelper.Create(true) : ResponseHelper.Create(false);
    }
}