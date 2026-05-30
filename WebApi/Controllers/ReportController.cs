using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;

namespace WebApi.Controllers;

/// <summary>
/// Controlador público para que los usuarios autenticados reporten contenido.
/// </summary>
[Route("api/reports")]
[ApiController]
[Authorize]
[Tags("Reportes")]
public class ReportController(
    IReportService reportService) : ApiControllerBase
{
    [HttpPost("create")]
    [EndpointSummary("Crear un reporte")]
    [EndpointDescription("Permite a un usuario autenticado reportar una publicación, cuenta o mensaje por infracción de normas.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReport([FromBody] CreatePublicReportRequest model)
    {
        var userId = GetRequiredCurrentUserId();

        var alreadyReported = await reportService.HasActiveReportAsync(userId, model.EntityType, model.EntityId);
        if (alreadyReported)
        {
            return ConflictEnvelope(ReportConstants.ALREADY_REPORTED);
        }

        var report = await reportService.CreateReportAsync(
             userId,
             model.EntityType,
             model.EntityId,
             model.Category,
             model.Description);

        return CreatedEnvelope(nameof(GetReportById), new { id = report.ReportId }, report);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Obtener reporte por ID")]
    [EndpointDescription("Obtiene un reporte propio específico por su identificador único.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportById(Guid id)
    {
        var userId = GetRequiredCurrentUserId();
        var report = await reportService.GetReportsAsync(null, 1, 0);

        // Buscar entre los reportes del usuario si le pertenece
        var ownReport = report.Data?.Find(r => r.ReportId == id && r.ReporterUserId == userId);
        if (ownReport is null)
        {
            return NotFoundEnvelope(ReportConstants.REPORT_NOT_FOUND);
        }

        return OkEnvelope(ownReport);
    }

    [HttpGet("mine")]
    [EndpointSummary("Obtener mis reportes")]
    [EndpointDescription("Obtiene la lista de todos los reportes creados por el usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var userId = GetRequiredCurrentUserId();
        var result = await reportService.GetReportsAsync(null, limit, offset);
        var mine = result.Data?.FindAll(r => r.ReporterUserId == userId);
        return OkEnvelope(mine ?? []);
    }

    [HttpGet("check")]
    [EndpointSummary("Verificar estado de reporte")]
    [EndpointDescription("Verifica si el usuario autenticado ya ha reportado previamente una entidad específica.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckReportStatus(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId)
    {
        var userId = GetRequiredCurrentUserId();
        var alreadyReported = await reportService.HasActiveReportAsync(userId, entityType, entityId);
        return OkEnvelope(new { alreadyReported });
    }
}

/// <summary>
/// Petición para crear un reporte de contenido.
/// </summary>
public class CreatePublicReportRequest
{
    /// <summary>
    /// Tipo de entidad a reportar: "Post", "User", "Message".
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// ID de la entidad reportada.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Categoría: "spam", "hate_speech", "harassment", "misinformation", "nudity", "violence", "copyright", "other".
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// Descripción opcional o detalles del reporte.
    /// </summary>
    public string? Description { get; set; }
}