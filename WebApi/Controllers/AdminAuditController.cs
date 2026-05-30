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
