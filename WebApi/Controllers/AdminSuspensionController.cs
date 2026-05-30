using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;
using WebApi.Filters;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para que los administradores gestionen suspensiones de usuarios.
/// </summary>
[Route("api/admin/suspensions")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Administración - Suspensiones")]
public class AdminSuspensionController(
    ISuspensionService suspensionService,
    IAuditService _) : ApiControllerBase
{
    [HttpPost("suspend")]
    [RequirePermission(PermissionConstants.UsersBan)]
    [AdminAudit("SUSPEND_USER", "User")]
    [EndpointSummary("Suspender un usuario")]
    [EndpointDescription("Suspende la cuenta de un usuario de forma temporal o permanente.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SuspendUser([FromBody] SuspendUserRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var suspension = await suspensionService.SuspendAsync(model.UserId, adminId, model.SuspensionType, model.Reason, model.EndsAt);
        return OkEnvelope(suspension);
    }

    [HttpPost("lift")]
    [RequirePermission(PermissionConstants.UsersBan)]
    [EndpointSummary("Levantar suspensión")]
    [EndpointDescription("Levanta o cancela una suspensión activa de un usuario antes de su fecha de expiración.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LiftSuspension([FromBody] LiftSuspensionRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var suspension = await suspensionService.LiftSuspensionAsync(model.SuspensionId, adminId);
        return OkEnvelope(suspension);
    }

    [HttpGet("history/{userId:guid}")]
    [RequirePermission(PermissionConstants.UsersBan)]
    [EndpointSummary("Obtener historial de suspensiones")]
    [EndpointDescription("Obtiene el historial completo de suspensiones de un usuario específico.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSuspensionHistory(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = await suspensionService.GetSuspensionHistoryAsync(userId, limit, offset);
        return OkEnvelope(rsp);
    }
}

/// <summary>
/// Petición para suspender a un usuario.
/// </summary>
public class SuspendUserRequest
{
    /// <summary>
    /// ID del usuario a suspender.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Tipo de suspensión (por ejemplo: TEMPORARY, PERMANENT).
    /// </summary>
    public string SuspensionType { get; set; } = null!;

    /// <summary>
    /// Motivo detallado de la suspensión.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Fecha de expiración (opcional, null si es permanente).
    /// </summary>
    public DateTime? EndsAt { get; set; }
}

/// <summary>
/// Petición para levantar una suspensión.
/// </summary>
public class LiftSuspensionRequest
{
    /// <summary>
    /// ID de la suspensión a levantar.
    /// </summary>
    public Guid SuspensionId { get; set; }
}
