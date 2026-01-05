using DocNestApp.Infrastructure;
using DocNestApp.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("OK"));

// Optional: quick DB check endpoint while wiring things
app.MapGet("/db-ping", async (AppDbContext db, CancellationToken ct) =>
{
    var canConnect = await db.Database.CanConnectAsync(ct);
    return Results.Ok(new { canConnect });
});

app.Run();