using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;

namespace WebApi.Controllers;

/// <summary>
/// Endpoint público para que usuarios autenticados reporten posts, cuentas o mensajes.
/// </summary>
[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportController(
    IReportService reportService) : ApiControllerBase
{
    /// <summary>
    /// Reportar un post, cuenta o mensaje.
    /// </summary>
    [HttpPost("create")]
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

    /// <summary>
    /// Consultar un reporte propio por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
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

    /// <summary>
    /// Listar mis reportes.
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var userId = GetRequiredCurrentUserId();
        var result = await reportService.GetReportsAsync(null, limit, offset);
        var mine = result.Data?.FindAll(r => r.ReporterUserId == userId);
        return OkEnvelope(mine ?? []);
    }

    /// <summary>
    /// Verificar si ya reporté una entidad.
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> CheckReportStatus(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId)
    {
        var userId = GetRequiredCurrentUserId();
        var alreadyReported = await reportService.HasActiveReportAsync(userId, entityType, entityId);
        return OkEnvelope(new { alreadyReported });
    }
}

public class CreatePublicReportRequest
{
    /// <summary>
    /// Tipo de entidad: "Post", "User", "Message".
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// ID de la entidad reportada.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Categoría: "spam", "hate_speech", "harassment", "misinformation",
    /// "nudity", "violence", "copyright", "other".
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// Descripción opcional del reporte.
    /// </summary>
    public string? Description { get; set; }
}