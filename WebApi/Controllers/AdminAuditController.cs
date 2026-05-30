using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para que los administradores revisen las bitácoras de auditoría.
/// </summary>
[Route("api/admin/audit")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Administración - Auditoría")]
public class AdminAuditController(
    IAuditService auditService) : ApiControllerBase
{
    /// <summary>
    /// Obtiene y filtra los registros o logs de auditoría generados por acciones administrativas.
    /// </summary>
    /// <param name="limit">Cantidad máxima de registros de auditoría a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="adminUserId">Filtro opcional por identificador de administrador ejecutor.</param>
    /// <param name="action">Filtro opcional por el nombre o tipo de acción realizada.</param>
    /// <param name="entityType">Filtro opcional por el tipo de entidad afectada (ej. "Post", "User").</param>
    /// <param name="dateFrom">Filtro opcional para obtener registros desde esta fecha y hora.</param>
    /// <param name="dateTo">Filtro opcional para obtener registros hasta esta fecha y hora.</param>
    /// <returns>Una lista paginada con los logs de auditoría que coinciden con los criterios.</returns>
    [HttpGet("logs")]
    [RequirePermission(PermissionConstants.AuditView)]
    [EndpointSummary("Obtener registros de auditoría")]
    [EndpointDescription("Permite listar y filtrar las acciones realizadas por los administradores en el sistema.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        [FromQuery] Guid? adminUserId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var rsp = await auditService.GetAuditLogsAsync(limit, offset, adminUserId, action, entityType, dateFrom, dateTo);
        return OkEnvelope(rsp);
    }
}
