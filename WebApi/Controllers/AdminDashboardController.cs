using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para el panel (dashboard) de métricas y estadísticas del administrador.
/// </summary>
[Route("api/admin/dashboard")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Administración - Métricas")]
public class AdminDashboardController(
    IDashboardService dashboardService) : ApiControllerBase
{
    /// <summary>
    /// Obtiene métricas y estadísticas generales de uso y estado de la plataforma.
    /// </summary>
    /// <returns>Las métricas generales del sistema recopiladas.</returns>
    [HttpGet("stats")]
    [RequirePermission(PermissionConstants.DashboardView)]
    [EndpointSummary("Obtener estadísticas del sistema")]
    [EndpointDescription("Obtiene métricas y estadísticas generales de uso y estado de la plataforma.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats()
    {
        var rsp = await dashboardService.GetStatsAsync();
        return OkEnvelope(rsp);
    }

    /// <summary>
    /// Fuerza la actualización y el recalculo manual de todas las estadísticas del sistema.
    /// </summary>
    /// <returns>Una respuesta indicando que el recalculo se completó correctamente.</returns>
    [HttpPost("recalculate")]
    [RequirePermission(PermissionConstants.DashboardView)]
    [EndpointSummary("Recalcular estadísticas")]
    [EndpointDescription("Fuerza la actualización y el recalculo manual de todas las estadísticas del sistema.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecalculateStats()
    {
        await dashboardService.RecalculateStatsAsync();
        return SuccessEnvelope("Estadísticas recalculadas correctamente");
    }
}
