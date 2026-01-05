namespace DocNestApp.IntegrationTests.Infra;

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public sealed class DocNestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _fileRoot;

    public DocNestApiFactory(string connectionString)
    {
        _connectionString = connectionString;
        _fileRoot = Path.Combine(Path.GetTempPath(), "docnest-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_fileRoot);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        
        builder.UseSetting("ConnectionStrings:docnest-db", _connectionString);
        builder.UseSetting("DocNest:AutoMigrate", "false");
        
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["DocNest:AutoMigrate"] = "false",
                ["ConnectionStrings:docnest-db"] = _connectionString,
                ["FileStore:RootPath"] = _fileRoot,

                ["Reminders:ScanIntervalSeconds"] = "999999"
            };

            config.AddInMemoryCollection(overrides);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { Directory.Delete(_fileRoot, recursive: true); } catch { /* ignore */ }
    }
}