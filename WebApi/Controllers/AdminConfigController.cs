using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/admin/config")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class AdminConfigController(
    IConfigService configService,
    IAuditService auditService) : ControllerBase
{
    [HttpGet("all")]
    [RequirePermission(PermissionConstants.ConfigView)]
    public IActionResult GetAllConfigs()
    {
        var rsp = configService.GetAllConfigsAsync();
        return Ok(rsp);
    }

    [HttpGet("{key}")]
    [RequirePermission(PermissionConstants.ConfigView)]
    public async Task<IActionResult> GetConfigByKey(string key)
    {
        var config = await configService.GetConfigAsync(key);
        if (config is null)
        {
            return NotFound();
        }
        return Ok(config);
    }

    [HttpPut("{key}")]
    [RequirePermission(PermissionConstants.ConfigEdit)]
    public async Task<IActionResult> UpdateConfig(string key, [FromBody] UpdateConfigRequest model)
    {
        var adminId = GetAdminId();
        var config = await configService.UpdateConfigAsync(key, model.Value, adminId);
        return Ok(config);
    }

    private Guid GetAdminId()
    {
        var claim = User.FindFirst(ClaimsConstants.USER_ID)?.Value
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);
        return Guid.Parse(claim);
    }
}

public class UpdateConfigRequest
{
    public string Value { get; set; } = null!;
}
