using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;
using WebApi.Filters;

namespace WebApi.Controllers;

[Route("api/admin/users")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class AdminUserController(
    IAdminService adminService,
    IAuditService _auditService,
    ICacheService _cacheService) : ApiControllerBase
{
    [HttpGet("list")]
    [RequirePermission(PermissionConstants.UsersView)]
    public async Task<IActionResult> ListUsers([FromQuery] int limit = 0, [FromQuery] int offset = 0, [FromQuery] string? fullName = null, [FromQuery] string? email = null, [FromQuery] bool? includeDeleted = null)
    {
        var rsp = await adminService.ListUsersAsync(limit, offset, fullName, email, includeDeleted);
        return OkEnvelope(rsp);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionConstants.UsersDelete)]
    [AdminAudit("SOFT_DELETE_USER", "User")]
    public async Task<IActionResult> SoftDeleteUser(Guid id, [FromQuery] string? reason = null)
    {
        var adminId = GetRequiredCurrentUserId();
        var user = await adminService.SoftDeleteUserAsync(id, adminId, reason);
        return OkEnvelope(user);
    }

    [HttpPost("{id:guid}/restore")]
    [RequirePermission(PermissionConstants.UsersDelete)]
    public async Task<IActionResult> RestoreUser(Guid id)
    {
        var user = await adminService.RestoreUserAsync(id);
        return OkEnvelope(user);
    }

    [HttpPost("{id:guid}/verify")]
    [RequirePermission(PermissionConstants.UsersVerify)]
    public async Task<IActionResult> VerifyUser(Guid id)
    {
        var user = await adminService.VerifyUserAsync(id);
        return OkEnvelope(user);
    }

    [HttpDelete("{id:guid}/verify")]
    [RequirePermission(PermissionConstants.UsersVerify)]
    public async Task<IActionResult> UnverifyUser(Guid id)
    {
        var user = await adminService.UnverifyUserAsync(id);
        return OkEnvelope(user);
    }

    [HttpPut("{id:guid}/role")]
    [RequirePermission(PermissionConstants.UsersRoles)]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleRequest model)
    {
        var user = await adminService.ChangeUserRoleAsync(id, model.RoleId);
        return OkEnvelope(user);
    }
}

public class ChangeRoleRequest
{
    public Guid RoleId { get; set; }
}
