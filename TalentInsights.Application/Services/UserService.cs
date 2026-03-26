using TalentInsights.Application.Helpers;
using TalentInsights.Application.Interfaces.Services;
using TalentInsights.Application.Models.DTOs;
using TalentInsights.Application.Models.Requests.User;
using TalentInsights.Application.Models.Responses;
using TalentInsights.Shared;
using TalentInsights.Shared.Helpers;

namespace TalentInsights.Application.Services;

public class UserService(Cache<UserDto> cache) : IUserService
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

        cache.Add(user.UserId.ToString(), user);

        return ResponseHelper.Create(user);
    }

    public GenericResponse<UserDto> Update(Guid userId, UpdateUserRequest model)
    {
        var user = cache.Get(userId.ToString());

        if (user is null)
        {
            return ResponseHelper.Create(new UserDto(), "Usuario no encontrado");
        }

        if (!string.IsNullOrWhiteSpace(model.FullName))
        {
            user.FullName = model.FullName;
        }

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            user.Email = model.Email;
        }

        return ResponseHelper.Create(user);
    }

    public GenericResponse<List<UserDto>> Get(int limit, int offset)
    {
        var users = cache.Get();

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? users.Count : limit;
        var page = users.Skip(normalizedOffset).Take(normalizedLimit).ToList();
        return ResponseHelper.Create(page);
    }

    public GenericResponse<UserDto?> Get(Guid userId)
    {
        var user = cache.Get(userId.ToString());
        return ResponseHelper.Create(user);
    }

    public GenericResponse<bool> ChangePassword(Guid userId, ChangePasswordUserRequest model)
    {
        var user = cache.Get(userId.ToString());

        if (user is null)
        {
            return ResponseHelper.Create(false, "Usuario no encontrado");
        }

        return ResponseHelper.Create(true, "Contraseña actualizada correctamente");
    }

    public GenericResponse<bool> Delete(Guid userId)
    {
        var user = cache.Get(userId.ToString());

        if (user is null)
        {
            return ResponseHelper.Create(false);
        }

        cache.Delete(userId.ToString());
        return ResponseHelper.Create(true);
    }
}
