using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Services;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared;
using Shared.Constants;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly SMTP _smtp;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailService> _logger;

    public EmailService(SMTP smtp, IServiceScopeFactory scopeFactory, ILogger<EmailService> logger)
    {
        _smtp = smtp;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string fullName)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.Welcome, email, new { fullName, email });
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.PasswordChanged, email, new { fullName, email });
    }

    public async Task SendPasswordResetEmailAsync(string email, string fullName, string otp)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.PasswordReset, email, new { fullName, email, otp });
    }

    private async Task SendTemplateEmailAsync(string templateName, string to, object variables)
    {
        try
        {
            // Crear un scope nuevo para evitar conflicto de DbContext
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var template = unitOfWork.EmailTemplates.GetByName(templateName);

            if (template is null)
            {
                _logger.LogError("Template de email no encontrado: {TemplateName}", templateName);
                return;
            }

            var subject = ReplaceVariables(template.Subject, variables);
            var body = ReplaceVariables(template.Body, variables);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Enviando email {TemplateName} a: {Email}", templateName, to);
            }

            await _smtp.SendEmailAsync(to, subject, body, isHtml: true);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Email {TemplateName} enviado exitosamente a: {Email}", templateName, to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email {TemplateName} a: {Email}", templateName, to);
            // No lanzamos la excepción para no fallar la operación principal
        }
    }

    private static string ReplaceVariables(string template, object variables)
    {
        var result = template;
        var type = variables.GetType();

        foreach (var prop in type.GetProperties())
        {
            var value = prop.GetValue(variables)?.ToString() ?? "";
            result = result.Replace($"{{{prop.Name}}}", value);
        }

        return result;
    }
}