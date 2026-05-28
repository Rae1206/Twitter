using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/follow")]
[ApiController]
[Authorize]
public class FollowController(IFollowService followService) : ApiControllerBase
{
    [HttpPost("{userId:guid}/follow")]
    public async Task<IActionResult> Follow(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await followService.FollowUser(currentUserId, userId);
        return SuccessEnvelope("Ahora sigues a este usuario");
    }

    [HttpDelete("{userId:guid}/unfollow")]
    public async Task<IActionResult> Unfollow(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await followService.UnfollowUser(currentUserId, userId);
        return SuccessEnvelope("Dejaste de seguir a este usuario");
    }

    [HttpGet("{userId:guid}/is-following")]
    public async Task<IActionResult> IsFollowing(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var isFollowing = await followService.IsFollowing(currentUserId, userId);
        return OkEnvelope(new { isFollowing });
    }

    [HttpGet("{userId:guid}/followers")]
    public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var followers = await followService.GetFollowers(userId, limit, offset);
        return OkEnvelope(followers);
    }

    [HttpGet("{userId:guid}/following")]
    public async Task<IActionResult> GetFollowing(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var following = await followService.GetFollowing(userId, limit, offset);
        return OkEnvelope(following);
    }

    [HttpGet("{userId:guid}/followers/count")]
    public async Task<IActionResult> GetFollowersCount(Guid userId)
    {
        var count = await followService.GetFollowersCount(userId);
        return OkEnvelope(count);
    }

    [HttpGet("{userId:guid}/following/count")]
    public async Task<IActionResult> GetFollowingCount(Guid userId)
    {
        var count = await followService.GetFollowingCount(userId);
        return OkEnvelope(count);
    }
}
