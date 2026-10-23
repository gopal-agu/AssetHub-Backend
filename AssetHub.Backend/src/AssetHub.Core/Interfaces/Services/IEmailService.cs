namespace AssetHub.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? ccEmail = null);
}
