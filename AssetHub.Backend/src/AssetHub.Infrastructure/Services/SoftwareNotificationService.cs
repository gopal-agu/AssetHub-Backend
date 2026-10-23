using AssetHub.Core.Common;
using AssetHub.Core.Entities;
using AssetHub.Core.Interfaces.Repositories;
using AssetHub.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AssetHub.Infrastructure.Services;

public class SoftwareNotificationService : ISoftwareNotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IActivityLogService _activity;
    private readonly ILogger<SoftwareNotificationService> _logger;

    public SoftwareNotificationService(
        IUnitOfWork uow,
        IEmailService emailService,
        IOptions<EmailSettings> emailOptions,
        IActivityLogService activity,
        ILogger<SoftwareNotificationService> logger)
    {
        _uow = uow;
        _emailService = emailService;
        _emailSettings = emailOptions.Value;
        _activity = activity;
        _logger = logger;
    }

    public async Task<int> CheckAndSendSoftwareExpiryAlertsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetDate = today.AddDays(7);

        // Fetch active subscriptions where ExpiryDate or NextRenewalDate is between today and today + 7 days
        var subscriptions = await _uow.SoftwareSubscriptions.Query()
            .Include(s => s.Vendor)
            .Where(s => s.Status == "Active" &&
                        ((s.ExpiryDate >= today && s.ExpiryDate <= targetDate) ||
                         (s.NextRenewalDate >= today && s.NextRenewalDate <= targetDate)))
            .ToListAsync();

        if (subscriptions.Count == 0)
        {
            _logger.LogInformation("No software subscriptions expiring or due for renewal within 7 days (checked for window {Today} to {TargetDate}).", today, targetDate);
            return 0;
        }

        var managementEmail = await ResolveManagementEmailAsync();
        if (string.IsNullOrWhiteSpace(managementEmail))
        {
            _logger.LogWarning("Cannot send software expiry alerts: No ManagementEmail is configured in appsettings and no Admin user email was found.");
            return 0;
        }

        int sentCount = 0;

        foreach (var item in subscriptions)
        {
            try
            {
                var (subject, htmlBody) = BuildEmailContent(item, today);
                var responsiblePersonEmail = await ResolveResponsiblePersonEmailAsync(item);

                await _emailService.SendEmailAsync(managementEmail, subject, htmlBody, ccEmail: null);

                sentCount++;
                await _activity.LogAsync($"7-Day Expiry/Renewal alert email sent for '{item.ServiceName}' ({item.AssetCode}) to management ({managementEmail}).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send 7-day notification email for software '{ServiceName}' ({AssetCode})", item.ServiceName, item.AssetCode);
            }
        }

        return sentCount;
    }

    public async Task<bool> SendSingleSoftwareReminderAsync(string softwareId)
    {
        var item = await _uow.SoftwareSubscriptions.Query()
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.Id == softwareId);

        if (item == null)
        {
            return false;
        }

        var managementEmail = await ResolveManagementEmailAsync();
        if (string.IsNullOrWhiteSpace(managementEmail))
        {
            throw new InvalidOperationException("Management email is not configured in EmailSettings or database.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var (subject, htmlBody) = BuildEmailContent(item, today, isManualReminder: true);
        var responsiblePersonEmail = await ResolveResponsiblePersonEmailAsync(item);

        await _emailService.SendEmailAsync(managementEmail, subject, htmlBody, ccEmail: null);
        await _activity.LogAsync($"Manual reminder email sent for software '{item.ServiceName}' ({item.AssetCode}) to management ({managementEmail}).");

        return true;
    }

    private async Task<string> ResolveManagementEmailAsync()
    {
        // CURRENT LOGIC (COMMENTED OUT):
        // if (!string.IsNullOrWhiteSpace(_emailSettings.ManagementEmail))
        // {
        //     return _emailSettings.ManagementEmail.Trim();
        // }

        // NEW LOGIC: Supports multiple email IDs configured in ManagementEmail (array or list)
        if (_emailSettings.ManagementEmail != null && _emailSettings.ManagementEmail.Count > 0)
        {
            var validEmails = _emailSettings.ManagementEmail
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .ToList();

            if (validEmails.Count > 0)
            {
                return string.Join(", ", validEmails);
            }
        }

        // Fallback: look for Admin or Manager in Users
        var admin = await _uow.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
        return admin?.Email?.Trim() ?? string.Empty;
    }

    private async Task<string?> ResolveResponsiblePersonEmailAsync(SoftwareSubscription item)
    {
        if (!string.IsNullOrWhiteSpace(item.ResponsiblePersonId))
        {
            var emp = await _uow.Employees.GetByIdAsync(item.ResponsiblePersonId);
            if (emp != null && !string.IsNullOrWhiteSpace(emp.Email))
            {
                return emp.Email.Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(item.ResponsiblePersonName))
        {
            var emp = await _uow.Employees.FirstOrDefaultAsync(e => e.Name == item.ResponsiblePersonName);
            if (emp != null && !string.IsNullOrWhiteSpace(emp.Email))
            {
                return emp.Email.Trim();
            }
        }

        return null;
    }

    private static (string Subject, string HtmlBody) BuildEmailContent(SoftwareSubscription s, DateOnly today, bool isManualReminder = false)
    {
        int daysToExpiry = s.ExpiryDate.DayNumber - today.DayNumber;
        int daysToRenewal = s.NextRenewalDate.DayNumber - today.DayNumber;

        string triggerReason;
        string deadlineInfo;

        if (daysToExpiry >= 0 && daysToExpiry <= 7)
        {
            triggerReason = $"Subscription Expiry in {daysToExpiry} day{(daysToExpiry == 1 ? "" : "s")}";
            deadlineInfo = $"Expiry Date: <strong>{s.ExpiryDate:yyyy-MM-dd}</strong> ({daysToExpiry} day{(daysToExpiry == 1 ? "" : "s")} left)";
        }
        else if (daysToRenewal >= 0 && daysToRenewal <= 7)
        {
            triggerReason = $"Renewal Due Date in {daysToRenewal} day{(daysToRenewal == 1 ? "" : "s")}";
            deadlineInfo = $"Next Renewal Date: <strong>{s.NextRenewalDate:yyyy-MM-dd}</strong> ({daysToRenewal} day{(daysToRenewal == 1 ? "" : "s")} left)";
        }
        else
        {
            triggerReason = "Renewal / Expiry Notice";
            deadlineInfo = $"Expiry: <strong>{s.ExpiryDate:yyyy-MM-dd}</strong> | Next Renewal: <strong>{s.NextRenewalDate:yyyy-MM-dd}</strong>";
        }

        string prefix = isManualReminder ? "[Manual Reminder]" : "[Action Required - 7 Days Alert]";
        string subject = $"{prefix} Software Service {triggerReason}: {s.ServiceName} ({s.AssetCode})";

        string htmlBody = $@"
        <div style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 8px; background-color: #ffffff;"">
            <div style=""border-bottom: 2px solid #6366f1; padding-bottom: 12px; margin-bottom: 20px;"">
                <h2 style=""color: #1e293b; margin: 0; font-size: 20px;"">AssetHub - Software Service Alert</h2>
                <span style=""display: inline-block; background-color: #fef3c7; color: #92400e; font-size: 12px; font-weight: 600; padding: 4px 10px; border-radius: 12px; margin-top: 8px;"">
                    {triggerReason}
                </span>
            </div>

            <p style=""color: #334155; font-size: 14px; line-height: 1.6;"">
                Dear Management Team,
            </p>
            <p style=""color: #334155; font-size: 14px; line-height: 1.6;"">
                This is an automated notification that the software service <strong>{s.ServiceName}</strong> ({s.AssetCode}) has an upcoming deadline requiring management attention:
            </p>

            <div style=""background-color: #f8fafc; border-left: 4px solid #6366f1; padding: 12px 16px; margin: 16px 0; font-size: 14px; color: #1e293b;"">
                {deadlineInfo}
            </div>

            <table style=""width: 100%; border-collapse: collapse; margin-top: 16px; font-size: 13px;"">
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b; width: 40%;"">Service Name</td>
                    <td style=""padding: 10px 0; color: #0f172a; font-weight: 600;"">{s.ServiceName}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b;"">Asset Code</td>
                    <td style=""padding: 10px 0; color: #0f172a; font-family: monospace;"">{s.AssetCode}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b;"">Category</td>
                    <td style=""padding: 10px 0; color: #0f172a;"">{s.Category}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b;"">Responsible Person</td>
                    <td style=""padding: 10px 0; color: #0f172a;"">{s.ResponsiblePersonName}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b;"">Vendor</td>
                    <td style=""padding: 10px 0; color: #0f172a;"">{s.VendorName ?? s.Vendor?.Name ?? "—"}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b;"">Billing Amount</td>
                    <td style=""padding: 10px 0; color: #0f172a; font-weight: 600;"">{s.Currency} {s.Amount:N2} ({s.BillingType})</td>
                </tr>
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b;"">Auto Renewal</td>
                    <td style=""padding: 10px 0; color: #0f172a;"">{(s.AutoRenewal ? "Yes" : "No")}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #f1f5f9;"">
                    <td style=""padding: 10px 0; color: #64748b;"">Next Renewal Date</td>
                    <td style=""padding: 10px 0; color: #0f172a;"">{s.NextRenewalDate:yyyy-MM-dd}</td>
                </tr>
                <tr>
                    <td style=""padding: 10px 0; color: #64748b;"">Expiry Date</td>
                    <td style=""padding: 10px 0; color: #0f172a;"">{s.ExpiryDate:yyyy-MM-dd}</td>
                </tr>
            </table>

            <p style=""margin-top: 24px; font-size: 13px; color: #64748b; line-height: 1.5;"">
                Please log into <a href=""https://asset.sdaemon.com"" style=""color: #6366f1; text-decoration: none;"">AssetHub Portal</a> to review renewal approval, update invoices, or adjust subscription statuses.
            </p>

            <div style=""margin-top: 24px; padding-top: 12px; border-top: 1px solid #e2e8f0; font-size: 11px; color: #94a3b8; text-align: center;"">
                AssetHub Automated IT Asset Management System &bull; Please do not reply directly to this email.
            </div>
        </div>";

        return (subject, htmlBody);
    }
}
