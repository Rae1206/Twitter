using Application.Models.DTOs;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IAdminService
{
    Task<GenericResponse<List<UserDto>>> ListUsersAsync(int limit, int offset, string? nickname = null, string? email = null, bool? includeDeleted = null);
    Task<UserDto> SoftDeleteUserAsync(Guid userId, Guid adminId, string? reason = null);
    Task<UserDto> RestoreUserAsync(Guid userId);
    Task<UserDto> VerifyUserAsync(Guid userId);
    Task<UserDto> UnverifyUserAsync(Guid userId);
    Task<UserDto> ChangeUserRoleAsync(Guid userId, Guid roleId);
}
