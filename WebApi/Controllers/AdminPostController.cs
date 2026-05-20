using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Twitter.Domain.Database.SqlServer;
using WebApi.Attributes;
using WebApi.Filters;

namespace WebApi.Controllers;

[Route("api/admin/posts")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class AdminPostController(
    IPostService postService,
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    IEmailService emailService) : ControllerBase
{
    [HttpGet("list")]
    [RequirePermission(PermissionConstants.PostsView)]
    public IActionResult ListPosts([FromQuery] int limit = 0, [FromQuery] int offset = 0, [FromQuery] Guid? userId = null)
    {
        var rsp = postService.Get(limit, offset, userId, null);
        return Ok(rsp);
    }

    [HttpPost("{id:guid}/flag")]
    [RequirePermission(PermissionConstants.PostsFlag)]
    public async Task<IActionResult> FlagPost(Guid id)
    {
        var post = unitOfWork.Posts.GetById(id);
        if (post is null)
        {
            return NotFound();
        }

        post.IsFlagged = true;
        unitOfWork.Update(post);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.PostsDelete)]
    [AdminAudit("SOFT_DELETE_POST", "Post")]
    public async Task<IActionResult> SoftDeletePost(Guid id, [FromQuery] string? reason = null)
    {
        var adminId = GetAdminId();
        var post = unitOfWork.Posts.GetById(id);
        if (post is null)
        {
            return NotFound();
        }

        post.DeletedAt = DateTime.UtcNow;
        post.DeletedByAdminId = adminId;
        post.DeletedReason = reason;
        unitOfWork.Update(post);
        await unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(post.User?.Email))
        {
            await emailService.SendPostRemovedAsync(post.User.Email, post.User.FullName, reason ?? "Violation of terms");
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    [RequirePermission(PermissionConstants.PostsDelete)]
    public async Task<IActionResult> RestorePost(Guid id)
    {
        var post = unitOfWork.Posts.GetById(id);
        if (post is null)
        {
            return NotFound();
        }

        post.DeletedAt = null;
        post.DeletedByAdminId = null;
        post.DeletedReason = null;
        unitOfWork.Update(post);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private Guid GetAdminId()
    {
        var claim = User.FindFirst(ClaimsConstants.USER_ID)?.Value
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);
        return Guid.Parse(claim);
    }
}
