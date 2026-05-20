using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;
using WebApi.Filters;

namespace WebApi.Controllers;

[Route("api/admin/suspensions")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class AdminSuspensionController(
    ISuspensionService suspensionService,
    IAuditService auditService) : ControllerBase
{
    [HttpPost("suspend")]
    [RequirePermission(PermissionConstants.UsersBan)]
    [AdminAudit("SUSPEND_USER", "User")]
    public async Task<IActionResult> SuspendUser([FromBody] SuspendUserRequest model)
    {
        var adminId = GetAdminId();
        var suspension = await suspensionService.SuspendAsync(model.UserId, adminId, model.SuspensionType, model.Reason, model.EndsAt);
        return Ok(suspension);
    }

    [HttpPost("lift")]
    [RequirePermission(PermissionConstants.UsersBan)]
    public async Task<IActionResult> LiftSuspension([FromBody] LiftSuspensionRequest model)
    {
        var adminId = GetAdminId();
        var suspension = await suspensionService.LiftSuspensionAsync(model.SuspensionId, adminId);
        return Ok(suspension);
    }

    [HttpGet("history/{userId:guid}")]
    [RequirePermission(PermissionConstants.UsersBan)]
    public IActionResult GetSuspensionHistory(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = suspensionService.GetSuspensionHistoryAsync(userId, limit, offset);
        return Ok(rsp);
    }

    private Guid GetAdminId()
    {
        var claim = User.FindFirst(ClaimsConstants.USER_ID)?.Value
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);
        return Guid.Parse(claim);
    }
}

public class SuspendUserRequest
{
    public Guid UserId { get; set; }
    public string SuspensionType { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime? EndsAt { get; set; }
}

public class LiftSuspensionRequest
{
    public Guid SuspensionId { get; set; }
}
