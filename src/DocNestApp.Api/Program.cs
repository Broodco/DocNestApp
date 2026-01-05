using DocNestApp.Infrastructure;
using DocNestApp.Infrastructure.Database;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var app = builder.Build();

if (app.Environment.IsDevelopment() &&
    builder.Configuration.GetValue("DocNest:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseFastEndpoints();
app.UseSwaggerGen();
app.MapGet("/health", () => Results.Ok("OK"));

app.MapGet("/db-ping", async (AppDbContext db, CancellationToken ct) =>
{
    var canConnect = await db.Database.CanConnectAsync(ct);
    return Results.Ok(new { canConnect });
});

app.Run();

public partial class Program { }