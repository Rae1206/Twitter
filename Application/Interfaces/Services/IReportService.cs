using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

public interface IReportService
{
    Task<ContentReport> CreateReportAsync(Guid reporterId, string targetType, string targetId, string reason);
    Task<ContentReport> AssignReportAsync(Guid reportId, Guid assignedTo);
    Task<ContentReport> ResolveReportAsync(Guid reportId, string resolution);
    Task<ContentReport> DismissReportAsync(Guid reportId, string reason);
    Task<GenericResponse<List<ContentReport>>> GetReportsAsync(string? status = null, int limit = 0, int offset = 0);
}
