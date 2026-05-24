using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

public interface ISuspensionService
{
    Task<UserSuspension> SuspendAsync(Guid userId, Guid adminId, string suspensionType, string reason, DateTime? endsAt = null);
    Task<UserSuspension> LiftSuspensionAsync(Guid suspensionId, Guid liftedByUserId);
    Task<GenericResponse<List<UserSuspension>>> GetSuspensionHistoryAsync(Guid userId, int limit = 0, int offset = 0);
}
