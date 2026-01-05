namespace DocNestApp.Worker.Reminders;

using Infrastructure.Database;
using DocNestApp.Domain.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public sealed class ExpiryReminderWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiryReminderWorker> logger,
    IOptions<ReminderOptions> options)
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
                await Tick(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ExpiryReminderWorker tick failed");
            }
        }
    }

    private async Task Tick(CancellationToken ct)
    {
        var utcNow = DateTime.UtcNow;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Find reminders due and not dispatched
        var dueReminders = await db.Reminders
            .Where(r => r.DispatchedAtUtc == null && r.DueAtUtc <= utcNow)
            .OrderBy(r => r.DueAtUtc)
            .Take(100)
            .ToListAsync(ct);

        foreach (var r in dueReminders)
        {
            // MVP "dispatch": log it
            logger.LogInformation("REMINDER: Document {DocumentId} expires on {ExpiresOn} (daysBefore={DaysBefore}) for user {UserId}",
                r.DocumentId, r.ExpiresOn, r.DaysBefore, r.UserId);

            r.MarkDispatched(utcNow);
        }

        if (dueReminders.Count > 0)
            await db.SaveChangesAsync(ct);

        // Generate new reminders for documents (idempotent without exceptions)
        foreach (var daysBefore in _options.DaysBefore.Distinct())
        {
            var targetDate = DateOnly.FromDateTime(utcNow.AddDays(daysBefore));

            var docs = await db.Documents
                .AsNoTracking()
                .Where(d => d.ExpiresOn == targetDate) // implies not null
                .Select(d => new { d.Id, d.UserId, ExpiresOn = d.ExpiresOn!.Value })
                .ToListAsync(ct);

            if (docs.Count == 0) continue;

            var docIds = docs.Select(d => d.Id).ToArray();

            // Fetch already-existing reminders for this policy
            var existingDocIds = await db.Reminders
                .AsNoTracking()
                .Where(r => r.DaysBefore == daysBefore && docIds.Contains(r.DocumentId))
                .Select(r => r.DocumentId)
                .ToListAsync(ct);

            var existingSet = existingDocIds.ToHashSet();

            foreach (var d in docs)
            {
                if (existingSet.Contains(d.Id))
                    continue;

                db.Reminders.Add(Reminder.Create(d.UserId, d.Id, d.ExpiresOn, daysBefore, utcNow));
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
