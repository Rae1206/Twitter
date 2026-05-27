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
    IAuditService _) : ApiControllerBase
{
    [HttpPost("suspend")]
    [RequirePermission(PermissionConstants.UsersBan)]
    [AdminAudit("SUSPEND_USER", "User")]
    public async Task<IActionResult> SuspendUser([FromBody] SuspendUserRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var suspension = await suspensionService.SuspendAsync(model.UserId, adminId, model.SuspensionType, model.Reason, model.EndsAt);
        return OkEnvelope(suspension);
    }

    [HttpPost("lift")]
    [RequirePermission(PermissionConstants.UsersBan)]
    public async Task<IActionResult> LiftSuspension([FromBody] LiftSuspensionRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var suspension = await suspensionService.LiftSuspensionAsync(model.SuspensionId, adminId);
        return OkEnvelope(suspension);
    }

    [HttpGet("history/{userId:guid}")]
    [RequirePermission(PermissionConstants.UsersBan)]
    public async Task<IActionResult> GetSuspensionHistory(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = await suspensionService.GetSuspensionHistoryAsync(userId, limit, offset);
        return OkEnvelope(rsp);
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
