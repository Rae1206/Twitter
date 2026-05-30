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
    /// <summary>
    /// Obtiene una lista paginada de reportes pendientes de revisión.
    /// </summary>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista paginada de reportes pendientes.</returns>
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

    /// <summary>
    /// Obtiene la lista completa e histórica de todos los reportes de contenido.
    /// </summary>
    /// <param name="limit">Cantidad máxima de reportes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista paginada de todos los reportes.</returns>
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

    /// <summary>
    /// Resuelve un reporte de contenido aplicando una resolución específica (ej. eliminando contenido).
    /// </summary>
    /// <param name="id">Identificador único del reporte a resolver.</param>
    /// <param name="model">Modelo de solicitud con la descripción detallada de la resolución.</param>
    /// <returns>Los detalles del reporte resuelto.</returns>
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

    /// <summary>
    /// Desestima o descarta un reporte de contenido sin aplicar sanciones.
    /// </summary>
    /// <param name="id">Identificador único del reporte a desestimar.</param>
    /// <param name="model">Modelo de solicitud con el motivo de desestimación.</param>
    /// <returns>Los detalles del reporte desestimado.</returns>
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
/// Modelo de solicitud para resolver un reporte de contenido en el panel de administración.
/// </summary>
public class ResolveReportRequest
{
    /// <summary>
    /// Descripción de la resolución o acción de moderación aplicada al reporte.
    /// </summary>
    public string? Resolution { get; set; }
}

/// <summary>
/// Modelo de solicitud para desestimar o descartar un reporte de contenido.
/// </summary>
public class DismissReportRequest
{
    /// <summary>
    /// Razón o justificación por la cual se desestima el reporte.
    /// </summary>
    public string? Reason { get; set; }
}