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
/// Controlador para que los administradores gestionen cuentas de usuario, roles y verificación.
/// </summary>
[Route("api/admin/users")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Administración - Usuarios")]
public class AdminUserController(
    IAdminService adminService,
    IAuditService _auditService,
    ICacheService _cacheService,
    IUnitOfWork unitOfWork) : ApiControllerBase
{
    [HttpGet("list")]
    [RequirePermission(PermissionConstants.UsersView)]
    [EndpointSummary("Listar todos los usuarios")]
    [EndpointDescription("Obtiene una lista paginada de usuarios con filtros por nombre, email y estado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListUsers([FromQuery] int limit = 0, [FromQuery] int offset = 0, [FromQuery] string? nickname = null, [FromQuery] string? email = null, [FromQuery] bool? includeDeleted = null)
    {
        var rsp = await adminService.ListUsersAsync(limit, offset, nickname, email, includeDeleted);
        return OkEnvelope(rsp);
    }

    [HttpGet("roles")]
    [RequirePermission(PermissionConstants.UsersRoles)]
    [EndpointSummary("Listar roles del sistema")]
    [EndpointDescription("Obtiene una lista de todos los roles activos en el sistema.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListRoles()
    {
        var roles = await unitOfWork.Roles.GetAllAsync(filter: r => r.IsActive);
        var result = roles.Select(r => new { r.RoleId, r.Name }).ToList();
        return OkEnvelope(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.UsersDelete)]
    [AdminAudit("SOFT_DELETE_USER", "User")]
    [EndpointSummary("Eliminación lógica de usuario")]
    [EndpointDescription("Marca a un usuario como eliminado sin borrar físicamente sus datos del sistema.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDeleteUser(Guid id, [FromQuery] string? reason = null)
    {
        var adminId = GetRequiredCurrentUserId();
        var user = await adminService.SoftDeleteUserAsync(id, adminId, reason);
        return OkEnvelope(user);
    }

    [HttpPost("{id:guid}/restore")]
    [RequirePermission(PermissionConstants.UsersDelete)]
    [EndpointSummary("Restaurar usuario eliminado")]
    [EndpointDescription("Restaura la cuenta de un usuario que fue previamente desactivada o eliminada de forma lógica.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreUser(Guid id)
    {
        var user = await adminService.RestoreUserAsync(id);
        return OkEnvelope(user);
    }

    [HttpPost("{id:guid}/verify")]
    [RequirePermission(PermissionConstants.UsersVerify)]
    [EndpointSummary("Verificar cuenta de usuario")]
    [EndpointDescription("Otorga la insignia o estado de verificado a la cuenta de un usuario.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyUser(Guid id)
    {
        var user = await adminService.VerifyUserAsync(id);
        return OkEnvelope(user);
    }

    [HttpDelete("{id:guid}/verify")]
    [RequirePermission(PermissionConstants.UsersVerify)]
    [EndpointSummary("Quitar verificación de usuario")]
    [EndpointDescription("Remueve la insignia o estado de verificado de la cuenta de un usuario.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnverifyUser(Guid id)
    {
        var user = await adminService.UnverifyUserAsync(id);
        return OkEnvelope(user);
    }

    [HttpPut("{id:guid}/role")]
    [RequirePermission(PermissionConstants.UsersRoles)]
    [EndpointSummary("Cambiar rol de usuario")]
    [EndpointDescription("Actualiza el rol asignado a un usuario en el sistema.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleRequest model)
    {
        var user = await adminService.ChangeUserRoleAsync(id, model.RoleId);
        return OkEnvelope(user);
    }
}

/// <summary>
/// Petición para cambiar el rol de un usuario.
/// </summary>
public class ChangeRoleRequest
{
    /// <summary>
    /// ID del nuevo rol a asignar.
    /// </summary>
    public Guid RoleId { get; set; }
}
