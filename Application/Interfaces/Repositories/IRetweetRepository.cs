using Application.Models.DTOs;

namespace Application.Interfaces.Repositories;

public interface IRetweetRepository
{
    RetweetDto Create(RetweetDto retweet);
    bool Delete(Guid retweetId);
    RetweetDto? GetById(Guid retweetId);
    List<RetweetDto> GetRetweetsByPost(Guid postId, int limit, int offset);
    List<RetweetDto> GetRetweetsByUser(Guid userId, int limit, int offset);
    bool Exists(Guid userId, Guid postId);
    int GetRetweetsCount(Guid postId);
}