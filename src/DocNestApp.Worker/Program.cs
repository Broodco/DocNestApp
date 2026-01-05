using DocNestApp.Infrastructure;
using DocNestApp.Worker.Reminders;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<ReminderOptions>(builder.Configuration.GetSection("Reminders"));
builder.Services.AddHostedService<ExpiryReminderWorker>();

var host = builder.Build();
host.Run();