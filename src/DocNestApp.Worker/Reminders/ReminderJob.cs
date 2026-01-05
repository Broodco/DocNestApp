namespace DocNestApp.Worker.Reminders;

using DocNestApp.Domain.Reminders;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

public sealed class ReminderJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderJob> _logger;
    private readonly ReminderOptions _options;

    public ReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ReminderJob> logger,
        ReminderOptions options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options;
    }

    public async Task RunOnceAsync(DateTime utcNow, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dueReminders = await db.Reminders
            .Where(r => r.DispatchedAtUtc == null && r.DueAtUtc <= utcNow)
            .OrderBy(r => r.DueAtUtc)
            .Take(100)
            .ToListAsync(ct);

        foreach (var r in dueReminders)
        {
            _logger.LogInformation(
                "REMINDER: Document {DocumentId} expires on {ExpiresOn} (daysBefore={DaysBefore}) for user {UserId}",
                r.DocumentId, r.ExpiresOn, r.DaysBefore, r.UserId);

            r.MarkDispatched(utcNow);
        }

        if (dueReminders.Count > 0)
            await db.SaveChangesAsync(ct);

        foreach (var daysBefore in _options.DaysBefore.Distinct())
        {
            if (daysBefore < 0) continue;

            var targetDate = DateOnly.FromDateTime(utcNow.AddDays(daysBefore));

            var docs = await db.Documents
                .AsNoTracking()
                .Where(d => d.ExpiresOn == targetDate) // implies not null
                .Select(d => new { d.Id, d.UserId, ExpiresOn = d.ExpiresOn!.Value })
                .ToListAsync(ct);

            if (docs.Count == 0) continue;

            var docIds = docs.Select(d => d.Id).ToArray();

            var existingDocIds = await db.Reminders
                .AsNoTracking()
                .Where(r => r.DaysBefore == daysBefore && docIds.Contains(r.DocumentId))
                .Select(r => r.DocumentId)
                .ToListAsync(ct);

            var existing = existingDocIds.ToHashSet();

            foreach (var d in docs)
            {
                if (existing.Contains(d.Id))
                    continue;

                db.Reminders.Add(Reminder.Create(d.UserId, d.Id, d.ExpiresOn, daysBefore, utcNow));
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
