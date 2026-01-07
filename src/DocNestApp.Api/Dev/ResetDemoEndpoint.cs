using DocNestApp.Infrastructure.Database;
using DocNestApp.Infrastructure.Demo;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DocNestApp.Api.Dev;

public sealed class ResetDemoEndpoint(
    AppDbContext db,
    DemoSeeder seeder,
    IConfiguration config,
    IHostEnvironment env)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/dev/reset-demo");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Reset demo data (Development only)";
            s.Description = "Deletes all documents, reminders and files, then re-seeds demo data.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!env.IsDevelopment())
            ThrowError("This endpoint is only available in Development.", StatusCodes.Status403Forbidden);

        var demoMode = config.GetValue<bool>("DocNest:DemoMode");
        if (!demoMode)
            ThrowError("DemoMode must be enabled to reset demo data.", StatusCodes.Status400BadRequest);

        await db.Reminders.ExecuteDeleteAsync(ct);
        await db.Documents.ExecuteDeleteAsync(ct);

        await DeleteDemoUserFilesAsync();

        await seeder.SeedIfNeededAsync(DateTime.UtcNow, ct);

        await Send.NoContentAsync(ct);
    }

    private Task DeleteDemoUserFilesAsync()
    {
        var userId = config.GetValue<Guid>("DocNest:DemoUserId");
        if (userId == Guid.Empty)
            return Task.CompletedTask;

        var rootPath = config.GetValue<string>("FileStore:RootPath");
        if (string.IsNullOrWhiteSpace(rootPath))
            return Task.CompletedTask;

        var userDir = Path.Combine(
            Path.GetFullPath(rootPath),
            "files",
            userId.ToString("N"));

        if (Directory.Exists(userDir))
            Directory.Delete(userDir, recursive: true);

        return Task.CompletedTask;
    }
}
