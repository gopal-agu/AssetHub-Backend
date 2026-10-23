using System.Net;
using System.Net.Mail;
using AssetHub.Core.Common;
using AssetHub.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AssetHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? ccEmail = null)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpServer) || string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            _logger.LogWarning("Email settings (SmtpServer or SenderEmail) are not fully configured in appsettings. Skipped email to '{ToEmail}' with subject '{Subject}'.", toEmail, subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Recipient email is empty. Skipped sending email with subject '{Subject}'.", subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = !string.IsNullOrWhiteSpace(_settings.Username)
                    ? new NetworkCredential(_settings.Username, _settings.Password)
                    : CredentialCache.DefaultNetworkCredentials
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            // OLD CODE (COMMENTED OUT):
            // message.To.Add(toEmail);

            // NEW CODE: Support multiple comma or semicolon separated recipient emails
            var toList = toEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var recipient in toList)
            {
                if (!string.IsNullOrWhiteSpace(recipient))
                {
                    message.To.Add(recipient);
                }
            }

            if (!string.IsNullOrWhiteSpace(ccEmail))
            {
                var ccList = ccEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var cc in ccList)
                {
                    if (!string.IsNullOrWhiteSpace(cc) && !toList.Contains(cc, StringComparer.OrdinalIgnoreCase))
                    {
                        message.CC.Add(cc);
                    }
                }
            }

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {ToEmail}. Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}. Subject: {Subject}", toEmail, subject);
            throw;
        }
    }
}
