using AssetHub.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AssetHub.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that runs daily to scan for software subscriptions
/// whose Expiry Date or Next Renewal Due Date is within 7 days, and sends
/// email notifications to the management team and responsible persons.
/// </summary>
public class SoftwareExpiryReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SoftwareExpiryReminderBackgroundService> _logger;

    // Check frequency: Once every 24 hours (with an initial startup delay)
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);

    public SoftwareExpiryReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SoftwareExpiryReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Software Expiry & Due Date Reminder Background Service is starting.");

        try
        {
            // Initial delay after service startup before running the first check
            await Task.Delay(InitialDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Running daily check for software subscriptions expiring or due in 7 days...");

                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<ISoftwareNotificationService>();

                int alertsSent = await notificationService.CheckAndSendSoftwareExpiryAlertsAsync();
                _logger.LogInformation("Software expiration check completed. {Count} alert(s) sent.", alertsSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled error occurred while checking software subscription expirations.");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Software Expiry & Due Date Reminder Background Service is stopping.");
    }
}
