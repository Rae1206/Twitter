using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio que permite a los administradores gestionar usuarios, restaurar cuentas, verificar perfiles y cambiar roles.
/// </summary>
public class AdminService(
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    ICacheService cacheService,
    ILogger<AdminService> logger) : IAdminService
{
    /// <summary>
    /// Obtiene de forma paginada y filtrada una lista de usuarios registrados en la plataforma.
    /// </summary>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="nickname">Filtro de búsqueda por apodo o nickname.</param>
    /// <param name="email">Filtro de búsqueda por correo electrónico.</param>
    /// <param name="includeDeleted">Indica si se deben incluir usuarios marcados como eliminados (no implementado completamente sin refactorizar repositorios).</param>
    /// <returns>Un sobre genérico de respuesta con la lista de usuarios en formato DTO.</returns>
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

    /// <summary>
    /// Realiza una desactivación o borrado lógico de un usuario por parte de un administrador, registrando la auditoría del cambio.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a eliminar.</param>
    /// <param name="adminId">Identificador único del administrador que realiza la acción.</param>
    /// <param name="reason">Razón o motivo explicativo de la desactivación.</param>
    /// <returns>Los detalles del usuario desactivado en forma de DTO.</returns>
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

    /// <summary>
    /// Restaura una cuenta de usuario que había sido desactivada de forma lógica previamente.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a restaurar.</param>
    /// <returns>Los detalles del usuario restaurado en forma de DTO.</returns>
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

    /// <summary>
    /// Otorga el estado de verificado/activo al usuario en el sistema.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a verificar.</param>
    /// <returns>Los detalles del usuario verificado en forma de DTO.</returns>
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

    /// <summary>
    /// Remueve el estado de verificado/activo al usuario en el sistema.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Los detalles del usuario modificado en forma de DTO.</returns>
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

    /// <summary>
    /// Asigna o añade un rol de seguridad nuevo a un usuario existente, e invalida la caché de sus permisos.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="roleId">Identificador único del rol a asignar.</param>
    /// <returns>Los detalles actualizados del usuario en forma de DTO.</returns>
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

    /// <summary>
    /// Mapea de forma interna la entidad User de base de datos a un objeto UserDto.
    /// </summary>
    /// <param name="entity">Entidad de base de datos a mapear.</param>
    /// <returns>El DTO de usuario mapeado.</returns>
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
