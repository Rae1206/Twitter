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
    IReportService reportService,
    IAuditService auditService) : ControllerBase
{
    [HttpGet("pending")]
    [RequirePermission(PermissionConstants.ReportsView)]
    public IActionResult GetPendingReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = reportService.GetReportsAsync("Pending", limit, offset);
        return Ok(rsp);
    }

    [HttpGet("all")]
    [RequirePermission(PermissionConstants.ReportsView)]
    public IActionResult GetAllReports([FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var rsp = reportService.GetReportsAsync(null, limit, offset);
        return Ok(rsp);
    }

    [HttpPost("create")]
    [RequirePermission(PermissionConstants.ReportsView)]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest model)
    {
        var reporterId = GetAdminId();
        var report = await reportService.CreateReportAsync(reporterId, model.TargetType, model.TargetId, model.Reason);
        return CreatedAtAction(nameof(GetAllReports), new { }, report);
    }

    [HttpPut("{id:guid}/assign")]
    [RequirePermission(PermissionConstants.ReportsAssign)]
    public async Task<IActionResult> AssignReport(Guid id, [FromBody] AssignReportRequest model)
    {
        var report = await reportService.AssignReportAsync(id, model.AssignedTo);
        return Ok(report);
    }

    [HttpPut("{id:guid}/resolve")]
    [RequirePermission(PermissionConstants.ReportsResolve)]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportRequest model)
    {
        var report = await reportService.ResolveReportAsync(id, model.Resolution);
        return Ok(report);
    }

    [HttpPut("{id:guid}/dismiss")]
    [RequirePermission(PermissionConstants.ReportsResolve)]
    public async Task<IActionResult> DismissReport(Guid id, [FromBody] DismissReportRequest model)
    {
        var report = await reportService.DismissReportAsync(id, model.Reason);
        return Ok(report);
    }

    private Guid GetAdminId()
    {
        var claim = User.FindFirst(ClaimsConstants.USER_ID)?.Value
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);
        return Guid.Parse(claim);
    }
}

public class CreateReportRequest
{
    public string TargetType { get; set; } = null!;
    public string TargetId { get; set; } = null!;
    public string Reason { get; set; } = null!;
}

public class AssignReportRequest
{
    public Guid AssignedTo { get; set; }
}

public class ResolveReportRequest
{
    public string Resolution { get; set; } = null!;
}

public class DismissReportRequest
{
    public string Reason { get; set; } = null!;
}
