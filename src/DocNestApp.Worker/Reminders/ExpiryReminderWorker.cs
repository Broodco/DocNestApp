namespace DocNestApp.Worker.Reminders;

using Microsoft.Extensions.Options;

public sealed class ExpiryReminderWorker(
    ILogger<ExpiryReminderWorker> logger,
    IOptions<ReminderOptions> options,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly ReminderOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiryReminderWorker started. Interval={Interval}s DaysBefore=[{Days}]",
            _options.ScanIntervalSeconds,
            string.Join(",", _options.DaysBefore));

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.ScanIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var job = scope.ServiceProvider.GetRequiredService<ReminderJob>();

                await job.RunOnceAsync(DateTime.UtcNow, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ExpiryReminderWorker tick failed");
            }
        }
    }
}