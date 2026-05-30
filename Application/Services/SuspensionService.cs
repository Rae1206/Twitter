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
/// Servicio de lógica de negocio para gestionar el bloqueo, suspensión y restauración de cuentas de usuario.
/// </summary>
public class SuspensionService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IAuditService auditService,
    ICacheService cacheService,
    ILogger<SuspensionService> logger) : ISuspensionService
{
    /// <summary>
    /// Suspende la cuenta de un usuario de forma temporal o permanente, registra la auditoría, limpia su caché de permisos y le envía una notificación por correo electrónico.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a suspender.</param>
    /// <param name="adminId">Identificador único del administrador que ejecuta la suspensión.</param>
    /// <param name="suspensionType">Tipo de suspensión ("Temporary" o "Permanent").</param>
    /// <param name="reason">Razón o motivo detallado de la sanción.</param>
    /// <param name="endsAt">Fecha y hora (UTC) de finalización de la suspensión si es temporal; null si es indefinida.</param>
    /// <returns>Los detalles del registro de suspensión creado en forma de DTO.</returns>
    public async Task<SuspensionDto> SuspendAsync(Guid userId, Guid adminId, string suspensionType, string reason, DateTime? endsAt = null)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Suspendiendo usuario {UserId} por admin {AdminId} | Tipo: {Type}", userId, adminId, suspensionType);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.IsSuspended = true;
        user.SuspendedUntil = endsAt;
        unitOfWork.Update(user);

        var suspension = new UserSuspension
        {
            SuspensionId = Guid.NewGuid(),
            UserId = userId,
            AdminUserId = adminId,
            SuspensionType = suspensionType,
            Reason = reason,
            EndsAt = endsAt,
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };
        unitOfWork.Create(suspension);

        await unitOfWork.SaveChangesAsync();

        await auditService.LogChangeAsync(adminId, "SUSPEND_USER", "User", userId.ToString(), null, new { suspensionType, reason, endsAt });

        cacheService.Delete($"perm:{userId}");

        if (suspensionType == "Permanent")
        {
            await emailService.SendAccountBannedPermanentAsync(user.Email, user.Nickname, reason);
        }
        else
        {
            await emailService.SendAccountSuspendedAsync(user.Email, user.Nickname, reason, endsAt);
        }

        return ToDto(suspension);
    }

    /// <summary>
    /// Levanta o cancela una suspensión activa sobre un usuario antes de su fecha programada de término.
    /// Invalida las cachés correspondientes y envía un correo informando la restauración de la cuenta.
    /// </summary>
    /// <param name="suspensionId">Identificador único de la suspensión a levantar.</param>
    /// <param name="liftedByUserId">Identificador único del administrador que levanta la sanción.</param>
    /// <returns>Los detalles de la suspensión levantada.</returns>
    public async Task<SuspensionDto> LiftSuspensionAsync(Guid suspensionId, Guid liftedByUserId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Levantando suspensión {SuspensionId} por admin {AdminId}", suspensionId, liftedByUserId);
        }

        var suspension = await unitOfWork.UserSuspensions.GetByIdAsync(suspensionId);
        if (suspension is null)
        {
            throw new ResourceNotFoundException("suspensión", suspensionId);
        }

        suspension.IsActive = false;
        suspension.LiftedByUserId = liftedByUserId;
        suspension.LiftedAt = DateTimeHelper.UtcNow();
        unitOfWork.Update(suspension);

        var user = await unitOfWork.Users.GetByIdAsync(suspension.UserId);
        if (user is not null)
        {
            user.IsSuspended = false;
            user.SuspendedUntil = null;
            unitOfWork.Update(user);
        }

        await unitOfWork.SaveChangesAsync();

        if (user is not null)
        {
            await emailService.SendAccountRestoredAsync(user.Email, user.Nickname);
            cacheService.Delete($"perm:{user.UserId}");
        }

        return ToDto(suspension);
    }

    /// <summary>
    /// Obtiene de forma paginada el historial completo de suspensiones registradas para un usuario específico.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad máxima de registros a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Un sobre genérico que envuelve la lista de suspensiones encontradas.</returns>
    public async Task<GenericResponse<List<SuspensionDto>>> GetSuspensionHistoryAsync(Guid userId, int limit = 0, int offset = 0)
    {
        var query = await unitOfWork.UserSuspensions.GetAllAsync(limit, offset, s => s.UserId == userId);
        return new GenericResponse<List<SuspensionDto>> { Data = query.Select(ToDto).ToList() };
    }

    /// <summary>
    /// Convierte o mapea de forma interna una entidad de base de datos UserSuspension a su representación de DTO.
    /// </summary>
    /// <param name="suspension">La entidad UserSuspension a mapear.</param>
    /// <returns>El DTO de suspensión correspondiente.</returns>
    private static SuspensionDto ToDto(UserSuspension suspension) => new()
    {
        SuspensionId = suspension.SuspensionId,
        UserId = suspension.UserId,
        AdminUserId = suspension.AdminUserId,
        SuspensionType = suspension.SuspensionType,
        Reason = suspension.Reason,
        EndsAt = suspension.EndsAt,
        IsActive = suspension.IsActive,
        LiftedByUserId = suspension.LiftedByUserId,
        LiftedAt = suspension.LiftedAt,
        CreatedAt = suspension.CreatedAt,
    };
}