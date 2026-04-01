using Application.Models.DTOs;

namespace Application.Interfaces.Repositories;

public interface ICommentRepository
{
    CommentDto Create(CommentDto comment);
    bool Delete(Guid commentId);
    CommentDto? GetById(Guid commentId);
    List<CommentDto> GetCommentsByPost(Guid postId, int limit, int offset);
    int GetCommentsCount(Guid postId);
}