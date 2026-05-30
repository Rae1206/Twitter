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
    /// <summary>
    /// Obtiene una lista paginada de usuarios en el sistema con filtros opcionales de nickname, email y estado de eliminación.
    /// </summary>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="nickname">Filtro opcional por apodo o nickname.</param>
    /// <param name="email">Filtro opcional por dirección de correo electrónico.</param>
    /// <param name="includeDeleted">Indica si se deben incluir los usuarios eliminados de forma lógica.</param>
    /// <returns>La lista paginada de usuarios obtenidos.</returns>
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

    /// <summary>
    /// Obtiene una lista de todos los roles activos registrados en el sistema.
    /// </summary>
    /// <returns>Una lista de roles con su ID y nombre.</returns>
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

    /// <summary>
    /// Realiza la desactivación o borrado lógico de una cuenta de usuario en el sistema.
    /// </summary>
    /// <param name="id">Identificador único del usuario a eliminar.</param>
    /// <param name="reason">Razón o justificación opcional de la desactivación.</param>
    /// <returns>Los detalles del usuario modificado.</returns>
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

    /// <summary>
    /// Restaura la cuenta de un usuario que fue previamente desactivada o eliminada de forma lógica.
    /// </summary>
    /// <param name="id">Identificador único del usuario a restaurar.</param>
    /// <returns>Los detalles del usuario restaurado.</returns>
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

    /// <summary>
    /// Otorga el estado o insignia de verificado a la cuenta de un usuario específico.
    /// </summary>
    /// <param name="id">Identificador único del usuario a verificar.</param>
    /// <returns>Los detalles del usuario verificado.</returns>
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

    /// <summary>
    /// Remueve la insignia o estado de verificado de la cuenta de un usuario específico.
    /// </summary>
    /// <param name="id">Identificador único del usuario a desverificar.</param>
    /// <returns>Los detalles del usuario modificado.</returns>
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

    /// <summary>
    /// Cambia o actualiza el rol de seguridad asignado a un usuario en el sistema.
    /// </summary>
    /// <param name="id">Identificador único del usuario al que se le cambiará el rol.</param>
    /// <param name="model">Modelo de solicitud con el identificador único del nuevo rol.</param>
    /// <returns>Los detalles del usuario con su nuevo rol asignado.</returns>
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
/// Modelo de solicitud para cambiar el rol de un usuario en el sistema.
/// </summary>
public class ChangeRoleRequest
{
    /// <summary>
    /// Identificador único del nuevo rol a asignar al usuario.
    /// </summary>
    public Guid RoleId { get; set; }
}
