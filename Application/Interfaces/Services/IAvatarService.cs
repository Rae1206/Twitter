using Application.Models.DTOs;
using Application.Models.Requests.Media;

namespace Application.Interfaces.Services;

public interface IAvatarService
{
    Task<UserDto> UploadProfilePhotoAsync(Guid userId, UploadMediaRequest request);
    Task<UserProfilePhotoDto> GetProfilePhotoAsync(Guid userId);
}
