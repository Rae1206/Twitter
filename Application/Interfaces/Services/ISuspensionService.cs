using Application.Models.DTOs;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface ISuspensionService
{
    Task<SuspensionDto> SuspendAsync(Guid userId, Guid adminId, string suspensionType, string reason, DateTime? endsAt = null);
    Task<SuspensionDto> LiftSuspensionAsync(Guid suspensionId, Guid liftedByUserId);
    Task<GenericResponse<List<SuspensionDto>>> GetSuspensionHistoryAsync(Guid userId, int limit = 0, int offset = 0);
}