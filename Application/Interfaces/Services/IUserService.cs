using Application.Models.DTOs;
using Application.Models.Requests.Media;
using Application.Models.Requests.User;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> Create(CreateUserRequest model);
    Task<UserDto> UpdateProfile(Guid userId, UpdateUserRequest model);
    GenericResponse<List<UserDto>> Get(int limit, int offset, string? fullName = null, string? email = null);
    UserDto Get(Guid userId);
    UserProfilePhotoDto GetProfilePhoto(Guid userId);
    Task ChangePassword(Guid userId, ChangePasswordUserRequest model);
    Task<UserDto> UploadProfilePhoto(Guid userId, UploadMediaRequest request);
    Task Delete(Guid userId);
    Task Restore(Guid userId);
}
