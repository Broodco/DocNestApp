namespace DocNestApp.IntegrationTests.Reminders;

using DocNestApp.Domain.Documents;
using DocNestApp.Domain.Reminders;
using Infrastructure.Database;
using Infra;
using DocNestApp.Worker.Reminders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class ReminderJobTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public ReminderJobTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task RunOnce_creates_reminder_for_doc_expiring_in_1_day()
    {
        await _postgres.ResetAsync();

        var now = new DateTime(2026, 01, 05, 12, 00, 00, DateTimeKind.Utc);
        var expiresOn = DateOnly.FromDateTime(now.AddDays(1));

        // Arrange: insert a Document directly (faster than HTTP here)
        await using (var db = CreateDb())
        {
            var doc = Document.Create(
                userId: DevUser.UserId,
                subjectId: DevUser.SubjectId,
                title: "Test expiring doc",
                type: "ID",
                expiresOn: expiresOn,
                utcNow: now);

            db.Documents.Add(doc);
            await db.SaveChangesAsync();
        }

        var sp = ReminderJobFactory.Build(_postgres.ConnectionString, new ReminderOptions
        {
            ScanIntervalSeconds = 999999,
            DaysBefore = [1]
        });

        // Act
        using (sp)
        using (var scope = sp.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<ReminderJob>();
            await job.RunOnceAsync(now, CancellationToken.None);
        }

        // Assert
        await using (var db = CreateDb())
        {
            var reminders = await db.Reminders.AsNoTracking().ToListAsync();
            reminders.Should().HaveCount(1);
            reminders[0].DaysBefore.Should().Be(1);
            reminders[0].ExpiresOn.Should().Be(expiresOn);
            reminders[0].DispatchedAtUtc.Should().BeNull();
        }
    }

    [Fact]
    public async Task RunOnce_marks_due_reminders_as_dispatched()
    {
        await _postgres.ResetAsync();

        var now = new DateTime(2026, 01, 05, 12, 00, 00, DateTimeKind.Utc);
        var expiresOn = DateOnly.FromDateTime(now.AddDays(1));

        Guid documentId;

        // Arrange: create a doc + a reminder already due
        await using (var db = CreateDb())
        {
            var doc = Document.Create(
                userId: DevUser.UserId,
                subjectId: DevUser.SubjectId,
                title: "Doc",
                type: "ID",
                expiresOn: expiresOn,
                utcNow: now);

            db.Documents.Add(doc);
            await db.SaveChangesAsync();
            documentId = doc.Id;

            var reminder = Reminder.Create(DevUser.UserId, documentId, expiresOn, daysBefore: 1, utcNow: now);

            // Force due
            typeof(Reminder)
                .GetProperty(nameof(Reminder.DueAtUtc))!
                .SetValue(reminder, now.AddMinutes(-1));

            db.Reminders.Add(reminder);
            await db.SaveChangesAsync();
        }

        var sp = ReminderJobFactory.Build(_postgres.ConnectionString, new ReminderOptions
        {
            ScanIntervalSeconds = 999999,
            DaysBefore = [1]
        });

        // Act
        using (sp)
        using (var scope = sp.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<ReminderJob>();
            await job.RunOnceAsync(now, CancellationToken.None);
        }

        // Assert
        await using (var db = CreateDb())
        {
            var r = await db.Reminders.AsNoTracking().SingleAsync();
            r.DocumentId.Should().Be(documentId);
            r.DispatchedAtUtc.Should().NotBeNull();
            r.DispatchedAtUtc.Should().Be(now);
        }
    }

    private AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .Options;

        return new AppDbContext(opts);
    }
}
