using Microsoft.Extensions.Logging;

namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string nickname);
    Task SendPasswordChangedNotificationAsync(string email, string nickname);
    Task SendPasswordResetEmailAsync(string email, string nickname, string resetToken);
    Task SendAccountSuspendedAsync(string email, string nickname, string reason, DateTime? endsAt);
    Task SendAccountBannedPermanentAsync(string email, string nickname, string reason);
    Task SendAccountRestoredAsync(string email, string nickname);
    Task SendPostRemovedAsync(string email, string nickname, string reason);
}