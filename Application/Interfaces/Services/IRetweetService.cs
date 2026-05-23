using System;
using System.Threading.Tasks;
using Application.Models.DTOs;
using Application.Models.Requests.Post;

namespace Application.Interfaces.Services;

public interface IRetweetService
{
    Task<PostDto> CreateRetweet(Guid postId, Guid userId, CreateRetweetRequest model);
}
