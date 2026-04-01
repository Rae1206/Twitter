using Application.Interfaces.Repositories;
using Application.Models.DTOs;

namespace Application.Repositories;

public class CommentRepository : ICommentRepository
{
    // TODO: Implement with EF Core

    public CommentDto Create(CommentDto comment)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Delete(Guid commentId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<CommentDto> GetCommentsByPost(Guid postId, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public int GetCommentsCount(Guid postId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public CommentDto? GetById(Guid commentId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }
}