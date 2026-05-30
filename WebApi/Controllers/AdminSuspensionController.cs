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
    /// <summary>
    /// Suspende la cuenta de un usuario de forma temporal o permanente en el sistema.
    /// </summary>
    /// <param name="model">Modelo de solicitud con los detalles del usuario, tipo de suspensión, motivo y fecha de expiración.</param>
    /// <returns>La suspensión creada.</returns>
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

    /// <summary>
    /// Levanta o cancela una suspensión activa de un usuario antes de su fecha de expiración programada.
    /// </summary>
    /// <param name="model">Modelo de solicitud con el identificador único de la suspensión a levantar.</param>
    /// <returns>Los detalles de la suspensión actualizada/levantada.</returns>
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

    /// <summary>
    /// Obtiene el historial completo de suspensiones (activas e inactivas) de un usuario específico de forma paginada.
    /// </summary>
    /// <param name="userId">Identificador único del usuario del que se consulta el historial.</param>
    /// <param name="limit">Cantidad máxima de registros de suspensión a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista paginada del historial de suspensiones del usuario.</returns>
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
/// Modelo de solicitud para suspender a un usuario en el sistema.
/// </summary>
public class SuspendUserRequest
{
    /// <summary>
    /// Identificador único del usuario a suspender.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Tipo de suspensión aplicada (por ejemplo: TEMPORARY, PERMANENT).
    /// </summary>
    public string SuspensionType { get; set; } = null!;

    /// <summary>
    /// Motivo detallado y justificado de la suspensión.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Fecha y hora (UTC) en la que expira la suspensión (null si es una suspensión permanente).
    /// </summary>
    public DateTime? EndsAt { get; set; }
}

/// <summary>
/// Modelo de solicitud para levantar la suspensión de un usuario.
/// </summary>
public class LiftSuspensionRequest
{
    /// <summary>
    /// Identificador único del registro de suspensión que se va a levantar.
    /// </summary>
    public Guid SuspensionId { get; set; }
}
