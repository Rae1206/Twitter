using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Services;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared;
using Shared.Constants;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio encargado de componer y enviar correos electrónicos dinámicos basados en plantillas SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly SMTP _smtp;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="EmailService"/>.
    /// </summary>
    /// <param name="smtp">Servicio SMTP para el envío físico de correos.</param>
    /// <param name="scopeFactory">Factoría de alcances (scope) para resolver dependencias transitorias como UnitOfWork.</param>
    /// <param name="logger">Servicio de logging para registro de eventos.</param>
    public EmailService(SMTP smtp, IServiceScopeFactory scopeFactory, ILogger<EmailService> logger)
    {
        _smtp = smtp;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Envía un correo electrónico de bienvenida a un usuario recién registrado.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del destinatario.</param>
    /// <param name="nickname">Apodo o nombre de usuario del destinatario.</param>
    /// <returns>Una tarea asíncrona que representa el proceso de envío.</returns>
    public async Task SendWelcomeEmailAsync(string email, string nickname)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.Welcome, email, new { fullName = nickname, email });
    }

    /// <summary>
    /// Envía una notificación por correo indicando que la contraseña del usuario ha sido cambiada.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del destinatario.</param>
    /// <param name="nickname">Apodo o nombre del usuario.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task SendPasswordChangedNotificationAsync(string email, string nickname)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.PasswordChanged, email, new { fullName = nickname, email });
    }

    /// <summary>
    /// Envía un correo electrónico que contiene el código OTP para reestablecer la contraseña.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del destinatario.</param>
    /// <param name="nickname">Apodo del usuario.</param>
    /// <param name="otp">Código OTP autogenerado.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task SendPasswordResetEmailAsync(string email, string nickname, string otp)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.PasswordReset, email, new { fullName = nickname, email, otp });
    }

    /// <summary>
    /// Envía una notificación por correo indicando que la cuenta del usuario ha sido suspendida temporalmente.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del destinatario.</param>
    /// <param name="nickname">Apodo del usuario.</param>
    /// <param name="reason">Motivo o justificación de la suspensión.</param>
    /// <param name="endsAt">Fecha y hora (UTC) programada para la finalización de la suspensión.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task SendAccountSuspendedAsync(string email, string nickname, string reason, DateTime? endsAt)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.AccountSuspended, email, new { fullName = nickname, email, reason, endsAt });
    }

    /// <summary>
    /// Envía un correo electrónico indicando que la cuenta del usuario ha sido suspendida permanentemente (baneada).
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del destinatario.</param>
    /// <param name="nickname">Apodo del usuario.</param>
    /// <param name="reason">Motivo o justificación del baneo permanente.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task SendAccountBannedPermanentAsync(string email, string nickname, string reason)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.AccountBannedPermanent, email, new { fullName = nickname, email, reason });
    }

    /// <summary>
    /// Envía una notificación por correo indicando que la suspensión de la cuenta ha sido levantada o restaurada.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del destinatario.</param>
    /// <param name="nickname">Apodo del usuario.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task SendAccountRestoredAsync(string email, string nickname)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.AccountRestored, email, new { fullName = nickname, email });
    }

    /// <summary>
    /// Envía una notificación por correo indicando que una publicación del usuario fue eliminada por infracción de normas.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del destinatario.</param>
    /// <param name="nickname">Apodo del usuario.</param>
    /// <param name="reason">Motivo de la eliminación de la publicación.</param>
    /// <returns>Una tarea asíncrona que representa el proceso.</returns>
    public async Task SendPostRemovedAsync(string email, string nickname, string reason)
    {
        await SendTemplateEmailAsync(EmailTemplateConstants.PostRemoved, email, new { fullName = nickname, email, reason });
    }

    /// <summary>
    /// Método privado que carga la plantilla de correo de la base de datos, reemplaza sus variables dinámicas y realiza el envío por SMTP.
    /// </summary>
    /// <param name="templateName">Nombre de la plantilla a cargar.</param>
    /// <param name="to">Correo del destinatario.</param>
    /// <param name="variables">Objeto anónimo cuyas propiedades se usarán como variables de reemplazo.</param>
    private async Task SendTemplateEmailAsync(string templateName, string to, object variables)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var template = await unitOfWork.EmailTemplates.GetByNameAsync(templateName);

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
        }
    }

    /// <summary>
    /// Reemplaza marcadores de posición del tipo "{PropName}" por sus valores reales obtenidos mediante reflexión en las propiedades del objeto de variables.
    /// </summary>
    /// <param name="template">Texto con formato/plantilla.</param>
    /// <param name="variables">Objeto contenedor de los valores de las variables.</param>
    /// <returns>El texto con las variables formateadas e inyectadas.</returns>
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
