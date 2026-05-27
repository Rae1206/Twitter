using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

public interface IReportService
{
    // === Público: usuarios normales ===
    Task<ContentReport> CreateReportAsync(Guid reporterUserId, string entityType, Guid entityId, string category, string? description);
    Task<bool> HasActiveReportAsync(Guid reporterUserId, string entityType, Guid entityId);
    Task<List<ContentReport>> GetReportsByEntityAsync(string entityType, Guid entityId, int limit = 0, int offset = 0);

    // === Admin: gestión ===
    Task<ContentReport> AssignReportAsync(Guid reportId, Guid adminId);
    Task<ContentReport> StartReviewAsync(Guid reportId);
    Task<ContentReport> ResolveReportAsync(Guid reportId, string resolution, Guid adminId);
    Task<ContentReport> DismissReportAsync(Guid reportId, string reason, Guid adminId);
    Task<GenericResponse<List<ContentReport>>> GetReportsAsync(string? status = null, int limit = 0, int offset = 0);
}