namespace DocNestApp.Worker.Reminders;

public sealed class ReminderOptions
{
    public int ScanIntervalSeconds { get; init; } = 30;
    public int[] DaysBefore { get; init; } = [30, 7, 1];
}