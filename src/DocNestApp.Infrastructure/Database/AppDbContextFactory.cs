using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DocNestApp.Infrastructure.Database;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var repoRoot = FindRepoRoot();
        var apiDir = Path.Combine(repoRoot, "src", "DocNestApp.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("docnest-db")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__docnest-db")
                               ?? throw new InvalidOperationException("Missing connection string 'docnest-db' for design-time migrations.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string FindRepoRoot()
    {
        // Start from current directory (works from repo root, src/, or a project folder)
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (dir is not null)
        {
            // pick one marker you know will exist in repo root
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")) ||
                File.Exists(Path.Combine(dir.FullName, "DocNestApp.sln")) ||
                File.Exists(Path.Combine(dir.FullName, "DocNestApp.slnx")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root (no .git / DocNestApp.sln / DocNestApp.slnx found).");
    }
}