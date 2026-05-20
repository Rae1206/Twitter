using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

public interface IAuditService
{
    Task LogChangeAsync(Guid adminId, string action, string entityType, string? entityId, object? oldValue, object? newValue, string? reason = null);
    GenericResponse<List<AdminAuditLog>> GetAuditLogsAsync(int limit, int offset, Guid? adminUserId = null, string? action = null, string? entityType = null, DateTime? dateFrom = null, DateTime? dateTo = null);
}
