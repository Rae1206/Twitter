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
    IAuditService auditService) : ApiControllerBase
{
    [HttpGet("all")]
    [RequirePermission(PermissionConstants.ConfigView)]
    public async Task<IActionResult> GetAllConfigs()
    {
        var rsp = await configService.GetAllConfigsAsync();
        return OkEnvelope(rsp);
    }

    [HttpGet("{key}")]
    [RequirePermission(PermissionConstants.ConfigView)]
    public async Task<IActionResult> GetConfigByKey(string key)
    {
        var config = await configService.GetConfigAsync(key);
        if (config is null)
        {
            return NotFoundEnvelope($"No se encontró la configuración '{key}'");
        }
        return OkEnvelope(config);
    }

    [HttpPut("{key}")]
    [RequirePermission(PermissionConstants.ConfigEdit)]
    public async Task<IActionResult> UpdateConfig(string key, [FromBody] UpdateConfigRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var config = await configService.UpdateConfigAsync(key, model.Value, adminId);
        return OkEnvelope(config);
    }
}

public class UpdateConfigRequest
{
    public string Value { get; set; } = null!;
}
