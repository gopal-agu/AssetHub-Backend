namespace AssetHub.Core.Interfaces.Services;

public interface ISoftwareNotificationService
{
    /// <summary>
    /// Checks all active software subscriptions and sends notification emails for any
    /// that are expiring or due for renewal within 7 days.
    /// </summary>
    /// <returns>Number of alert notifications processed and sent.</returns>
    Task<int> CheckAndSendSoftwareExpiryAlertsAsync();

    /// <summary>
    /// Sends a reminder email for a specific software subscription to management and responsible person.
    /// </summary>
    Task<bool> SendSingleSoftwareReminderAsync(string softwareId);
}
