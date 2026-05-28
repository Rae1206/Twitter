using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Services;

public class AdminService(
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    ICacheService cacheService,
    ILogger<AdminService> logger) : IAdminService
{
    public async Task<GenericResponse<List<UserDto>>> ListUsersAsync(int limit, int offset, string? nickname = null, string? email = null, bool? includeDeleted = null)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Listando usuarios | Limit: {Limit}, Offset: {Offset}, IncludeDeleted: {IncludeDeleted}", limit, offset, includeDeleted);
        }

        var users = await unitOfWork.Users.GetAllAsync(limit, offset, nickname, email);

        if (includeDeleted == true)
        {
            logger.LogWarning("includeDeleted=true requiere IgnoreQueryFilters; no soportado en esta iteración sin refactor de repositorios");
        }

        var dtos = users.Select(MapToDto).ToList();
        return new GenericResponse<List<UserDto>> { Data = dtos };
    }

    public async Task<UserDto> SoftDeleteUserAsync(Guid userId, Guid adminId, string? reason = null)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Soft delete de usuario {UserId} por admin {AdminId}", userId, adminId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        var oldValue = new { user.DeletedAt, user.DeletedByAdminId };

        user.DeletedAt = DateTimeHelper.UtcNow();
        user.DeletedByAdminId = adminId;

        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        await auditService.LogChangeAsync(adminId, "SOFT_DELETE_USER", "User", userId.ToString(), oldValue, new { user.DeletedAt, user.DeletedByAdminId }, reason);

        cacheService.Delete($"perm:{userId}");

        return MapToDto(user);
    }

    public async Task<UserDto> RestoreUserAsync(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Restaurando usuario {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.DeletedAt = null;
        user.DeletedByAdminId = null;

        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> VerifyUserAsync(Guid userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.IsActive = true;
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UnverifyUserAsync(Guid userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.IsActive = false;
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> ChangeUserRoleAsync(Guid userId, Guid roleId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        var role = await unitOfWork.Roles.GetByIdAsync(roleId);
        if (role is null)
        {
            throw new ResourceNotFoundException("rol", roleId);
        }

        var alreadyHasRole = user.UserRoles?.Any(ur => ur.RoleId == roleId) ?? false;
        if (alreadyHasRole)
        {
            throw new BadRequestException($"El usuario ya tiene el rol '{role.Name}' asignado.");
        }

        var userRole = new UserRole
        {
            UserRoleId = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTimeHelper.UtcNow()
        };
        unitOfWork.Create(userRole);
        await unitOfWork.SaveChangesAsync();

        cacheService.Delete($"perm:{userId}");

        return MapToDto(user);
    }

    private static UserDto MapToDto(User entity) => new()
    {
        UserId = entity.UserId,
        Nickname = entity.Nickname,
        Email = entity.Email,
        IsActive = entity.IsActive,
        IsSuspended = entity.IsSuspended,
        IsShadowBanned = entity.IsShadowBanned,
        DeletedAt = entity.DeletedAt,
        Roles = entity.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>(),
        CreatedAt = entity.CreatedAt
    };
}
