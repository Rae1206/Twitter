using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/admin/reports")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class AdminReportController(
    IReportService reportService) : ApiControllerBase
{
    [HttpGet("pending")]
    [RequirePermission(PermissionConstants.ReportsView)]
    public async Task<IActionResult> GetPendingReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = await reportService.GetReportsAsync(ReportConstants.STATUS_PENDING, limit, offset);
        return OkEnvelope(rsp);
    }

    [HttpGet("all")]
    [RequirePermission(PermissionConstants.ReportsView)]
    public async Task<IActionResult> GetAllReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = await reportService.GetReportsAsync(null, limit, offset);
        return OkEnvelope(rsp);
    }

    [HttpPut("{id:guid}/resolve")]
    [RequirePermission(PermissionConstants.ReportsResolve)]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var report = await reportService.ResolveReportAsync(id, model.Resolution, adminId);
        return OkEnvelope(report);
    }

    [HttpPut("{id:guid}/dismiss")]
    [RequirePermission(PermissionConstants.ReportsResolve)]
    public async Task<IActionResult> DismissReport(Guid id, [FromBody] DismissReportRequest model)
    {
        var adminId = GetRequiredCurrentUserId();
        var report = await reportService.DismissReportAsync(id, model.Reason, adminId);
        return OkEnvelope(report);
    }
}

public class ResolveReportRequest
{
    public string? Resolution { get; set; }
}

public class DismissReportRequest
{
    public string? Reason { get; set; }
}