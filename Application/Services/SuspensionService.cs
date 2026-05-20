using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Services;

public class SuspensionService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IAuditService auditService,
    ICacheService cacheService,
    ILogger<SuspensionService> logger) : ISuspensionService
{
    public async Task<UserSuspension> SuspendAsync(Guid userId, Guid adminId, string suspensionType, string reason, DateTime? endsAt = null)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Suspendiendo usuario {UserId} por admin {AdminId} | Tipo: {Type}", userId, adminId, suspensionType);
        }

        var user = unitOfWork.Users.GetById(userId);
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
            await emailService.SendAccountBannedPermanentAsync(user.Email, user.FullName, reason);
        }
        else
        {
            await emailService.SendAccountSuspendedAsync(user.Email, user.FullName, reason, endsAt);
        }

        return suspension;
    }

    public async Task<UserSuspension> LiftSuspensionAsync(Guid suspensionId, Guid liftedByUserId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Levantando suspensión {SuspensionId} por admin {AdminId}", suspensionId, liftedByUserId);
        }

        var suspension = unitOfWork.UserSuspensions.GetById(suspensionId);
        if (suspension is null)
        {
            throw new ResourceNotFoundException("suspensión", suspensionId);
        }

        suspension.IsActive = false;
        suspension.LiftedByUserId = liftedByUserId;
        suspension.LiftedAt = DateTimeHelper.UtcNow();
        unitOfWork.Update(suspension);

        var user = unitOfWork.Users.GetById(suspension.UserId);
        if (user is not null)
        {
            user.IsSuspended = false;
            user.SuspendedUntil = null;
            unitOfWork.Update(user);
        }

        await unitOfWork.SaveChangesAsync();

        if (user is not null)
        {
            await emailService.SendAccountRestoredAsync(user.Email, user.FullName);
            cacheService.Delete($"perm:{user.UserId}");
        }

        return suspension;
    }

    public GenericResponse<List<UserSuspension>> GetSuspensionHistoryAsync(Guid userId, int limit = 0, int offset = 0)
    {
        var query = unitOfWork.UserSuspensions.GetAll(limit, offset, s => s.UserId == userId);
        return new GenericResponse<List<UserSuspension>> { Data = query };
    }
}
