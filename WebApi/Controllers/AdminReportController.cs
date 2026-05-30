using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para gestionar y resolver reportes de contenido.
/// </summary>
[Route("api/admin/reports")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Administración - Reportes")]
public class AdminReportController(
    IReportService reportService) : ApiControllerBase
{
    [HttpGet("pending")]
    [RequirePermission(PermissionConstants.ReportsView)]
    [EndpointSummary("Obtener reportes pendientes")]
    [EndpointDescription("Obtiene una lista paginada de reportes pendientes de revisión.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = await reportService.GetReportsAsync(ReportConstants.STATUS_PENDING, limit, offset);
        return OkEnvelope(rsp);
    }

    [HttpGet("all")]
    [RequirePermission(PermissionConstants.ReportsView)]
    [EndpointSummary("Obtener todos los reportes")]
    [EndpointDescription("Obtiene la lista completa e histórica de reportes de contenido.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = await reportService.GetReportsAsync(null, limit, offset);
        return OkEnvelope(rsp);
    }

    [HttpPut("{id:guid}/resolve")]
    [RequirePermission(PermissionConstants.ReportsResolve)]
    [EndpointSummary("Resolver un reporte")]
    [EndpointDescription("Resuelve un reporte de contenido aplicando una resolución específica.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var report = await reportService.ResolveReportAsync(id, model.Resolution, adminId);
        return OkEnvelope(report);
    }

    [HttpPut("{id:guid}/dismiss")]
    [RequirePermission(PermissionConstants.ReportsResolve)]
    [EndpointSummary("Rechazar/desestimar reporte")]
    [EndpointDescription("Desestima o descarta un reporte de contenido sin aplicar sanciones.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DismissReport(Guid id, [FromBody] DismissReportRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var report = await reportService.DismissReportAsync(id, model.Reason, adminId);
        return OkEnvelope(report);
    }
}

/// <summary>
/// Petición para resolver un reporte.
/// </summary>
public class ResolveReportRequest
{
    /// <summary>
    /// Descripción de la resolución aplicada.
    /// </summary>
    public string? Resolution { get; set; }
}

/// <summary>
/// Petición para desestimar un reporte.
/// </summary>
public class DismissReportRequest
{
    /// <summary>
    /// Razón por la cual se desestima el reporte.
    /// </summary>
    public string? Reason { get; set; }
}