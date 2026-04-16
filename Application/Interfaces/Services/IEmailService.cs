using Microsoft.Extensions.Logging;

namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string fullName);
    Task SendPasswordChangedNotificationAsync(string email, string fullName);
    Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken);
}