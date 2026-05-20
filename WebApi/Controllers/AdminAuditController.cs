using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/admin/audit")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class AdminAuditController(
    IAuditService auditService) : ControllerBase
{
    [HttpGet("logs")]
    [RequirePermission(PermissionConstants.AuditView)]
    public IActionResult GetAuditLogs(
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        [FromQuery] Guid? adminUserId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var rsp = auditService.GetAuditLogsAsync(limit, offset, adminUserId, action, entityType, dateFrom, dateTo);
        return Ok(rsp);
    }
}
