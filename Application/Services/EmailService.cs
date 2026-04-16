using Microsoft.Extensions.Logging;
using Application.Interfaces.Services;
using Shared;
using Shared.Constants;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly SMTP _smtp;
    private readonly ILogger<EmailService> _logger;

    public EmailService(SMTP smtp, ILogger<EmailService> logger)
    {
        _smtp = smtp;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string fullName)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Enviando email de bienvenida a: {Email}", email);
        }

        var subject = "Bienvenido a Twitter";
        var body = $@"
            <h1>¡Bienvenido a Twitter, {fullName}!</h1>
            <p>Nos alegra mucho que te hayas unido a nuestra plataforma.</p>
            <p>Ahora puedes:</p>
            <ul>
                <li>Crear y compartir tus pensamientos</li>
                <li>Seguir a otros usuarios</li>
                <li>Interactuar con publicaciones</li>
            </ul>
            <p>¡Empieza a explorar y conectar con personas!</p>
        ";

        await _smtp.SendEmailAsync(email, subject, body, isHtml: true);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Email de bienvenida enviado exitosamente a: {Email}", email);
        }
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Enviando notificación de cambio de contraseña a: {Email}", email);
        }

        var subject = "Tu contraseña ha sido cambiada";
        var body = $@"
            <h1>Notificación de seguridad</h1>
            <p>Hola {fullName},</p>
            <p>Tu contraseña ha sido cambiada exitosamente.</p>
            <p>Si no fuiste tú quien realizó este cambio, por favor contacta con soporte inmediatamente.</p>
        ";

        await _smtp.SendEmailAsync(email, subject, body, isHtml: true);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Notificación de contraseña enviada a: {Email}", email);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Enviando email de recuperación de contraseña a: {Email}", email);
        }

        var subject = "Recupera tu contraseña";
        var body = $@"
            <h1>Recuperación de contraseña</h1>
            <p>Hola {fullName},</p>
            <p>Has solicitado recuperar tu contraseña.</p>
            <p>Tu token de recuperación es: <strong>{resetToken}</strong></p>
            <p>Este token expire en 24 horas.</p>
            <p>Si no solicitaste este cambio, ignora este email.</p>
        ";

        await _smtp.SendEmailAsync(email, subject, body, isHtml: true);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Email de recuperación enviado a: {Email}", email);
        }
    }
}