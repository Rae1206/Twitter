using Application.Models.DTOs;
using Application.Models.Requests.Media;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

public interface IMediaService
{
    Task<MediaUploadDto> UploadAsync(UploadMediaRequest request, Guid userId);
    Task<PostMedia?> GetByIdAsync(Guid mediaId);
    Task<List<PostMedia>> GetByPostIdAsync(Guid postId);
    Task DeleteAsync(Guid mediaId, Guid userId);
    Task CleanupOrphansAsync(TimeSpan maxAge);
}
