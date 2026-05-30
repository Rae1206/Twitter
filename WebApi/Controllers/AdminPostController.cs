using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Twitter.Domain.Interfaces;
using WebApi.Attributes;
using WebApi.Filters;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para que los administradores moderen publicaciones (marcar, eliminar, restaurar).
/// </summary>
[Route("api/admin/posts")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Administración - Publicaciones")]
public class AdminPostController(
    IPostService postService,
    IUnitOfWork unitOfWork,
    IAuditService _,
    IEmailService emailService) : ApiControllerBase
{
    [HttpGet("list")]
    [RequirePermission(PermissionConstants.PostsView)]
    [EndpointSummary("Listar todas las publicaciones (Moderación)")]
    [EndpointDescription("Obtiene una lista de publicaciones para fines de moderación, con filtros opcionales.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult ListPosts([FromQuery] int limit = 0, [FromQuery] int offset = 0, [FromQuery] Guid? userId = null)
    {
        var rsp = postService.Get(limit, offset, userId, null);
        return OkEnvelope(rsp);
    }

    [HttpPost("{id:guid}/flag")]
    [RequirePermission(PermissionConstants.PostsFlag)]
    [EndpointSummary("Marcar publicación como sospechosa")]
    [EndpointDescription("Marca una publicación específica con una bandera de advertencia o sospecha.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagPost(Guid id)
    {
        var post = await unitOfWork.Posts.GetByIdAsync(id);
        if (post is null)
        {
            return NotFoundEnvelope("Publicación no encontrada");
        }

        post.IsFlagged = true;
        unitOfWork.Update(post);
        await unitOfWork.SaveChangesAsync();

        return SuccessEnvelope("Publicación marcada correctamente");
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.PostsDelete)]
    [AdminAudit("SOFT_DELETE_POST", "Post")]
    [EndpointSummary("Eliminación lógica de publicación")]
    [EndpointDescription("Realiza un borrado lógico de una publicación y opcionalmente envía un correo al autor explicando la razón.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDeletePost(Guid id, [FromQuery] string? reason = null)
    {
        var adminId = GetRequiredCurrentUserId();
        var post = await unitOfWork.Posts.GetByIdAsync(id);
        if (post is null)
        {
            return NotFoundEnvelope("Publicación no encontrada");
        }

        post.DeletedAt = DateTime.UtcNow;
        post.DeletedByAdminId = adminId;
        post.DeletedReason = reason;
        unitOfWork.Update(post);
        await unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(post.User?.Email))
        {
            await emailService.SendPostRemovedAsync(post.User.Email, post.User.Nickname, reason ?? "Violation of terms");
        }

        return SuccessEnvelope("Publicación eliminada correctamente");
    }

    [HttpPost("{id:guid}/restore")]
    [RequirePermission(PermissionConstants.PostsDelete)]
    [EndpointSummary("Restaurar publicación eliminada")]
    [EndpointDescription("Restaura una publicación que fue previamente eliminada por un administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestorePost(Guid id)
    {
        var post = await unitOfWork.Posts.GetByIdAsync(id);
        if (post is null)
        {
            return NotFoundEnvelope("Publicación no encontrada");
        }

        post.DeletedAt = null;
        post.DeletedByAdminId = null;
        post.DeletedReason = null;
        unitOfWork.Update(post);
        await unitOfWork.SaveChangesAsync();

        return SuccessEnvelope("Publicación restaurada correctamente");
    }
}
