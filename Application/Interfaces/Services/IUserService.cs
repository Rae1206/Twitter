using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IUserService
{
    UserDto Create(CreateUserRequest model);
    UserDto Update(Guid userId, UpdateUserRequest model);
    GenericResponse<List<UserDto>> Get(int limit, int offset, string? fullName = null, string? email = null);
    UserDto Get(Guid userId);
    void ChangePassword(Guid userId, ChangePasswordUserRequest model);
    void Delete(Guid userId);
}
