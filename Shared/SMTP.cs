using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Shared.Constants;
using MimeKit;

namespace Shared;

/// <summary>
/// Utilidad para envío de emails mediante SMTP (usa MailKit)
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
        var host = Environment.GetEnvironmentVariable("SMTP__Host")
            ?? _configuration[ConfigurationConstants.SMTP_HOST]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_HOST);
        var port = int.Parse(Environment.GetEnvironmentVariable("SMTP__Port")
            ?? _configuration[ConfigurationConstants.SMTP_PORT]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_PORT));
        var user = Environment.GetEnvironmentVariable("SMTP__User")
            ?? _configuration[ConfigurationConstants.SMTP_USER]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_USER);
        var password = Environment.GetEnvironmentVariable("SMTP__Password")
            ?? _configuration[ConfigurationConstants.SMTP_PASSWORD]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_PASSWORD);
        var from = Environment.GetEnvironmentVariable("SMTP__From")
            ?? _configuration[ConfigurationConstants.SMTP_FROM]
            ?? throw new InvalidOperationException(ConfigurationConstants.SMTP_FROM);

        Console.WriteLine($"[SMTP] Host: {host}, Port: {port}, User: {user}, From: {from}");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(from, from));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;

        message.Body = isHtml
            ? new BodyBuilder { HtmlBody = body }.ToMessageBody()
            : new TextPart("plain") { Text = body };

        // Puerto 465 = SSL implícito, Puerto 587 = STARTTLS
        var secureSocket = port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(host, port, secureSocket);
        await client.AuthenticateAsync(user, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}