namespace DocNestApp.Domain.Reminders;

public sealed class Reminder
{
    private Reminder() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid DocumentId { get; private set; }

    public DateOnly ExpiresOn { get; private set; }
    public int DaysBefore { get; private set; }

    public DateTime DueAtUtc { get; private set; }     // when the reminder should fire
    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DispatchedAtUtc { get; private set; } // set when “sent” (logged) by worker

    public static Reminder Create(Guid userId, Guid documentId, DateOnly expiresOn, int daysBefore, DateTime utcNow)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (documentId == Guid.Empty) throw new ArgumentException("DocumentId is required.", nameof(documentId));
        if (daysBefore <= 0) throw new ArgumentException("DaysBefore must be > 0.", nameof(daysBefore));

        var dueAt = expiresOn.ToDateTime(TimeOnly.MinValue).AddDays(-daysBefore);

        return new Reminder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentId = documentId,
            ExpiresOn = expiresOn,
            DaysBefore = daysBefore,
            DueAtUtc = DateTime.SpecifyKind(dueAt, DateTimeKind.Utc),
            CreatedAtUtc = utcNow
        };
    }

    public void MarkDispatched(DateTime utcNow)
    {
        DispatchedAtUtc = utcNow;
    }
}