using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> Create(CreateUserRequest model);
    Task<UserDto> Update(Guid userId, UpdateUserRequest model);
    GenericResponse<List<UserDto>> Get(int limit, int offset, string? fullName = null, string? email = null);
    UserDto Get(Guid userId);
    Task ChangePassword(Guid userId, ChangePasswordUserRequest model);
    Task Delete(Guid userId);
    Task Restore(Guid userId);
}
