using DocNestApp.Infrastructure;
using DocNestApp.Worker.Reminders;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<ReminderOptions>(builder.Configuration.GetSection("Reminders"));
builder.Services.AddScoped(sp =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ReminderOptions>>().Value;
    return new ReminderJob(
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<ReminderJob>>(),
        opts);
});

builder.Services.AddHostedService<ExpiryReminderWorker>();

var host = builder.Build();
host.Run();