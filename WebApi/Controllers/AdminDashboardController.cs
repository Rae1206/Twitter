using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/admin/dashboard")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class AdminDashboardController(
    IDashboardService dashboardService) : ApiControllerBase
{
    [HttpGet("stats")]
    [RequirePermission(PermissionConstants.DashboardView)]
    public IActionResult GetStats()
    {
        var rsp = dashboardService.GetStatsAsync();
        return OkEnvelope(rsp);
    }

    [HttpPost("recalculate")]
    [RequirePermission(PermissionConstants.DashboardView)]
    public async Task<IActionResult> RecalculateStats()
    {
        await dashboardService.RecalculateStatsAsync();
        return SuccessEnvelope("Estadísticas recalculadas correctamente");
    }
}
