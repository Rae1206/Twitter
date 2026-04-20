using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Shared.Constants;

namespace Shared;

/// <summary>
/// Utilidad para envío de emails mediante SMTP
/// </summary>
public class SMTP
{
    private readonly IConfiguration _configuration;

    public SMTP(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Envía un email mediante SMTP
    /// </summary>
    /// <param name="to">Dirección de correo del destinatario</param>
    /// <param name="subject">Asunto del correo</param>
    /// <param name="body">Cuerpo del correo (puede incluir HTML)</param>
    /// <param name="isHtml">Indica si el cuerpo es HTML</param>
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var host = _configuration[ConfigurationConstants.SMTP_HOST]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_HOST);
        var port = int.Parse(_configuration[ConfigurationConstants.SMTP_PORT]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_PORT));
        var user = _configuration[ConfigurationConstants.SMTP_USER]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_USER);
        var password = _configuration[ConfigurationConstants.SMTP_PASSWORD]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_PASSWORD);
        var from = _configuration[ConfigurationConstants.SMTP_FROM]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_FROM);

        Console.WriteLine($"[SMTP] Host: {host}, Port: {port}, User: {user}, From: {from}");

        // Puerto 587 usa STARTTLS, puerto 465 usa SSL implícito
        bool useSsl = port == 465 || port == 587;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, password),
            EnableSsl = useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            TargetName = "STARTTLS" // Necesario para algunos servidores SMTP
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }

    /// <summary>
    /// Valida certificados SSL (temporal: acepta todos para testing)
    /// </summary>
    private static bool ValidateServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // Para desarrollo: aceptar todos los certificados
        // En producción, implementar validación apropiada
        return true;
    }
}
