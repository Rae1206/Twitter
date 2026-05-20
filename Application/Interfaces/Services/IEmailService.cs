using Microsoft.Extensions.Logging;

namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string fullName);
    Task SendPasswordChangedNotificationAsync(string email, string fullName);
    Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken);
    Task SendAccountSuspendedAsync(string email, string fullName, string reason, DateTime? endsAt);
    Task SendAccountBannedPermanentAsync(string email, string fullName, string reason);
    Task SendAccountRestoredAsync(string email, string fullName);
    Task SendPostRemovedAsync(string email, string fullName, string reason);
}