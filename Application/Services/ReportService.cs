using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Helpers;
using Twitter.Domain.Exceptions;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Services;

public class ReportService(
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    ILogger<ReportService> logger) : IReportService
{
    private static readonly HashSet<string> ValidEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ReportConstants.ENTITY_TYPE_POST,
        ReportConstants.ENTITY_TYPE_USER,
        ReportConstants.ENTITY_TYPE_MESSAGE
    };

    private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        ReportConstants.CATEGORY_SPAM,
        ReportConstants.CATEGORY_HATE_SPEECH,
        ReportConstants.CATEGORY_HARASSMENT,
        ReportConstants.CATEGORY_MISINFORMATION,
        ReportConstants.CATEGORY_NUDITY,
        ReportConstants.CATEGORY_VIOLENCE,
        ReportConstants.CATEGORY_COPYRIGHT,
        ReportConstants.CATEGORY_OTHER
    };

    // === Público: usuarios normales ===

    public async Task<ContentReport> CreateReportAsync(Guid reporterUserId, string entityType, Guid entityId, string category, string? description)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Creating report: User {ReporterUserId} reporting {EntityType} {EntityId}", reporterUserId, entityType, entityId);
        }

        if (!ValidEntityTypes.Contains(entityType))
        {
            throw new BadRequestException(ReportConstants.INVALID_ENTITY_TYPE);
        }

        if (!ValidCategories.Contains(category))
        {
            throw new BadRequestException(ReportConstants.INVALID_CATEGORY);
        }

        var reporter = await unitOfWork.Users.GetByIdAsync(reporterUserId);
        if (reporter is null)
        {
            throw new ResourceNotFoundException("user", reporterUserId);
        }

        // Verificar que no haya un reporte activo del mismo usuario sobre la misma entidad
        var alreadyReported = await unitOfWork.ContentReports.HasActiveReportAsync(reporterUserId, entityType, entityId);
        if (alreadyReported)
        {
            throw new ConflictException(ReportConstants.ALREADY_REPORTED);
        }

        // Determinar prioridad por categoría
        byte priority = category switch
        {
            ReportConstants.CATEGORY_VIOLENCE or ReportConstants.CATEGORY_HATE_SPEECH => ReportConstants.PRIORITY_HIGH,
            ReportConstants.CATEGORY_HARASSMENT or ReportConstants.CATEGORY_NUDITY => ReportConstants.PRIORITY_MEDIUM,
            _ => ReportConstants.PRIORITY_LOW
        };

        var report = new ContentReport
        {
            ReportId = Guid.NewGuid(),
            ReporterUserId = reporterUserId,
            EntityType = entityType,
            EntityId = entityId,
            Category = category,
            Description = description,
            Status = ReportConstants.STATUS_PENDING,
            Priority = priority,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(report);
        await unitOfWork.SaveChangesAsync();

        // Auto-flag si el tipo es Post y alcanza el umbral
        if (entityType == ReportConstants.ENTITY_TYPE_POST)
        {
            var post = await unitOfWork.Posts.GetByIdAsync(entityId);
            if (post is not null)
            {
                post.ReportCount += 1;
                if (post.ReportCount >= ReportConstants.DEFAULT_FLAG_THRESHOLD)
                {
                    post.IsFlagged = true;
                }
                unitOfWork.Update(post);
                await unitOfWork.SaveChangesAsync();
            }
        }

        return report;
    }

    public async Task<bool> HasActiveReportAsync(Guid reporterUserId, string entityType, Guid entityId)
    {
        return await unitOfWork.ContentReports.HasActiveReportAsync(reporterUserId, entityType, entityId);
    }

    public async Task<List<ContentReport>> GetReportsByEntityAsync(string entityType, Guid entityId, int limit = 0, int offset = 0)
    {
        return await unitOfWork.ContentReports.GetByEntityAsync(entityType, entityId, limit, offset);
    }


    // === Admin: gestión ===


    public async Task<ContentReport> ResolveReportAsync(Guid reportId, string resolution, Guid adminId)
    {
        var report = await unitOfWork.ContentReports.GetByIdAsync(reportId);
        if (report is null)
        {
            throw new ResourceNotFoundException("reporte", reportId);
        }

        report.Resolution = resolution;
        report.Status = ReportConstants.STATUS_RESOLVED;
        report.ResolvedAt = DateTimeHelper.UtcNow();
        report.ResolvedByAdminId = adminId;
        unitOfWork.Update(report);
        await unitOfWork.SaveChangesAsync();

        return report;
    }

    public async Task<ContentReport> DismissReportAsync(Guid reportId, string reason, Guid adminId)
    {
        var report = await unitOfWork.ContentReports.GetByIdAsync(reportId);
        if (report is null)
        {
            throw new ResourceNotFoundException("reporte", reportId);
        }

        report.Resolution = reason;
        report.Status = ReportConstants.STATUS_DISMISSED;
        report.ResolvedAt = DateTimeHelper.UtcNow();
        report.ResolvedByAdminId = adminId;
        unitOfWork.Update(report);
        await unitOfWork.SaveChangesAsync();

        return report;
    }

    public async Task<GenericResponse<List<AdminReportDto>>> GetReportsAsync(string? status = null, int limit = 0, int offset = 0)
    {
        List<ContentReport> reports;
        if (!string.IsNullOrWhiteSpace(status))
        {
            reports = await unitOfWork.ContentReports.GetByStatusAsync(status, limit, offset);
        }
        else
        {
            reports = await unitOfWork.ContentReports.GetAllAsync(limit, offset);
        }

        var dtos = new List<AdminReportDto>();

        if (reports.Count > 0)
        {
            // Batch-fetch posts to avoid N+1 queries
            var postIds = reports
                .Where(r => string.Equals(r.EntityType, ReportConstants.ENTITY_TYPE_POST, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.EntityId)
                .Distinct()
                .ToList();

            var postsMap = new Dictionary<Guid, Post>();
            if (postIds.Count > 0)
            {
                var posts = await unitOfWork.Posts.GetAllAsync(filter: p => postIds.Contains(p.PostId));
                postsMap = posts.ToDictionary(p => p.PostId);
            }

            // Batch-fetch messages to avoid N+1 queries
            var messageIds = reports
                .Where(r => string.Equals(r.EntityType, ReportConstants.ENTITY_TYPE_MESSAGE, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.EntityId)
                .Distinct()
                .ToList();

            var messagesMap = new Dictionary<Guid, Message>();
            if (messageIds.Count > 0)
            {
                var messages = await unitOfWork.Messages.GetAllAsync(filter: m => messageIds.Contains(m.MessageId));
                messagesMap = messages.ToDictionary(m => m.MessageId);
            }

            foreach (var r in reports)
            {
                var dto = new AdminReportDto
                {
                    ReportId = r.ReportId,
                    ReporterUserId = r.ReporterUserId,
                    EntityType = r.EntityType,
                    EntityId = r.EntityId,
                    Category = r.Category,
                    Reason = r.Category,
                    Description = r.Description,
                    Status = r.Status,
                    Priority = r.Priority,
                    Resolution = r.Resolution,
                    ResolvedAt = r.ResolvedAt,
                    ResolvedByAdminId = r.ResolvedByAdminId,
                    CreatedAt = r.CreatedAt
                };

                if (string.Equals(r.EntityType, ReportConstants.ENTITY_TYPE_POST, StringComparison.OrdinalIgnoreCase))
                {
                    dto.PostId = r.EntityId;
                    if (postsMap.TryGetValue(r.EntityId, out var post))
                    {
                        dto.ReportedUserId = post.UserId;
                    }
                }
                else if (string.Equals(r.EntityType, ReportConstants.ENTITY_TYPE_USER, StringComparison.OrdinalIgnoreCase))
                {
                    dto.ReportedUserId = r.EntityId;
                }
                else if (string.Equals(r.EntityType, ReportConstants.ENTITY_TYPE_MESSAGE, StringComparison.OrdinalIgnoreCase))
                {
                    if (messagesMap.TryGetValue(r.EntityId, out var message))
                    {
                        dto.ReportedUserId = message.SenderId;
                    }
                }

                dtos.Add(dto);
            }
        }

        return new GenericResponse<List<AdminReportDto>> { Data = dtos };
    }
}