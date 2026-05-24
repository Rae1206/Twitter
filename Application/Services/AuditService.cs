using System.Text.Json;
using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Helpers;

namespace Application.Services;

public class AuditService(
    IUnitOfWork unitOfWork,
    ILogger<AuditService> logger) : IAuditService
{
    public async Task LogChangeAsync(Guid adminId, string action, string entityType, string? entityId, object? oldValue, object? newValue, string? reason = null)
    {
        try
        {
            var log = new AdminAuditLog
            {
                AuditLogId = Guid.NewGuid(),
                AdminUserId = adminId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = oldValue is not null ? JsonSerializer.Serialize(oldValue) : null,
                NewValue = newValue is not null ? JsonSerializer.Serialize(newValue) : null,
                Reason = reason,
                CreatedAt = DateTimeHelper.UtcNow()
            };

            unitOfWork.Create(log);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al escribir en audit log | Action: {Action}, Admin: {AdminId}", action, adminId);
        }
    }

    public async Task<GenericResponse<List<AdminAuditLog>>> GetAuditLogsAsync(int limit, int offset, Guid? adminUserId = null, string? action = null, string? entityType = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var logs = await unitOfWork.AdminAuditLogs.GetPagedAsync(limit, offset, adminUserId, action, entityType, dateFrom, dateTo);
        return new GenericResponse<List<AdminAuditLog>> { Data = logs };
    }
}
