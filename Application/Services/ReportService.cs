using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Services;

public class ReportService(
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    ILogger<ReportService> logger) : IReportService
{
    public async Task<ContentReport> CreateReportAsync(Guid reporterId, string targetType, string targetId, string reason)
    {
        var report = new ContentReport
        {
            ReportId = Guid.NewGuid(),
            ReporterId = reporterId,
            TargetType = targetType,
            TargetId = targetId,
            Reason = reason,
            Status = "Pending",
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(report);
        await unitOfWork.SaveChangesAsync();

        // Auto-increment report count if target is a post
        if (targetType == "Post")
        {
            if (Guid.TryParse(targetId, out var postId))
            {
                var post = unitOfWork.Posts.GetById(postId);
                if (post is not null)
                {
                    post.ReportCount += 1;
                    if (post.ReportCount >= 5)
                    {
                        post.IsFlagged = true;
                    }
                    unitOfWork.Update(post);
                    await unitOfWork.SaveChangesAsync();
                }
            }
        }

        return report;
    }

    public async Task<ContentReport> AssignReportAsync(Guid reportId, Guid assignedTo)
    {
        var report = unitOfWork.ContentReports.GetById(reportId);
        if (report is null)
        {
            throw new ResourceNotFoundException("reporte", reportId);
        }

        report.AssignedTo = assignedTo;
        report.Status = "Assigned";
        unitOfWork.Update(report);
        await unitOfWork.SaveChangesAsync();

        return report;
    }

    public async Task<ContentReport> ResolveReportAsync(Guid reportId, string resolution)
    {
        var report = unitOfWork.ContentReports.GetById(reportId);
        if (report is null)
        {
            throw new ResourceNotFoundException("reporte", reportId);
        }

        report.Resolution = resolution;
        report.Status = "Resolved";
        report.ResolvedAt = DateTimeHelper.UtcNow();
        unitOfWork.Update(report);
        await unitOfWork.SaveChangesAsync();

        return report;
    }

    public async Task<ContentReport> DismissReportAsync(Guid reportId, string reason)
    {
        var report = unitOfWork.ContentReports.GetById(reportId);
        if (report is null)
        {
            throw new ResourceNotFoundException("reporte", reportId);
        }

        report.Resolution = reason;
        report.Status = "Dismissed";
        report.ResolvedAt = DateTimeHelper.UtcNow();
        unitOfWork.Update(report);
        await unitOfWork.SaveChangesAsync();

        return report;
    }

    public GenericResponse<List<ContentReport>> GetReportsAsync(string? status = null, int limit = 0, int offset = 0)
    {
        List<ContentReport> reports;
        if (!string.IsNullOrWhiteSpace(status))
        {
            reports = unitOfWork.ContentReports.GetByStatusAsync(status, limit, offset).Result;
        }
        else
        {
            reports = unitOfWork.ContentReports.GetAll(limit, offset);
        }

        return new GenericResponse<List<ContentReport>> { Data = reports };
    }
}
