using System;
using System.Threading.Tasks;
using Application.Models.DTOs;
using Application.Models.Requests.Post;

namespace Application.Interfaces.Services;

public interface ICommentService
{
    Task<PostDto> CreateComment(Guid parentPostId, Guid userId, CreateCommentRequest model);
}
