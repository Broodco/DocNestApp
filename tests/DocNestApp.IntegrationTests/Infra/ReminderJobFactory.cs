namespace DocNestApp.IntegrationTests.Infra;

using Infrastructure.Database;
using Worker.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ReminderJobFactory
{
    public static ServiceProvider Build(string connectionString, ReminderOptions options)
    {
        var services = new ServiceCollection();

        services.AddLogging(b => b.AddDebug().AddConsole());

        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));

        services.AddSingleton(options);
        services.AddScoped<ReminderJob>();

        return services.BuildServiceProvider();
    }
}