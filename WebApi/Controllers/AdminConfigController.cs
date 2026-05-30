using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para gestionar configuraciones del sistema desde el panel de administración.
/// </summary>
[Route("api/admin/config")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Administración - Configuración")]
public class AdminConfigController(
    IConfigService configService,
    IAuditService _) : ApiControllerBase
{
    [HttpGet("all")]
    [RequirePermission(PermissionConstants.ConfigView)]
    [EndpointSummary("Obtener todas las configuraciones")]
    [EndpointDescription("Obtiene la lista completa de configuraciones clave-valor del sistema.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllConfigs()
    {
        var rsp = await configService.GetAllConfigsAsync();
        return OkEnvelope(rsp);
    }

    [HttpGet("{key}")]
    [RequirePermission(PermissionConstants.ConfigView)]
    [EndpointSummary("Obtener configuración por clave")]
    [EndpointDescription("Obtiene el valor de una configuración del sistema específica usando su clave única.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [EndpointSummary("Actualizar configuración")]
    [EndpointDescription("Modifica el valor de una configuración del sistema específica usando su clave.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateConfig(string key, [FromBody] UpdateConfigRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var config = await configService.UpdateConfigAsync(key, model.Value, adminId);
        return OkEnvelope(config);
    }
}

/// <summary>
/// Modelo para actualizar una configuración.
/// </summary>
public class UpdateConfigRequest
{
    /// <summary>
    /// Nuevo valor para la configuración.
    /// </summary>
    public string Value { get; set; } = null!;
}
