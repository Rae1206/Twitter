using Application.Models.DTOs;

namespace Application.Interfaces.Repositories;

public interface ILikeRepository
{
    LikeDto Create(LikeDto like);
    bool Delete(Guid userId, Guid postId);
    LikeDto? GetById(Guid likeId);
    List<LikeDto> GetLikesByPost(Guid postId, int limit, int offset);
    bool Exists(Guid userId, Guid postId);
    int GetLikesCount(Guid postId);
}