using DocNestApp.Application.Abstractions.Storage;
using DocNestApp.Infrastructure.Database;
using DocNestApp.Infrastructure.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocNestApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("docnest-db")
            ?? throw new InvalidOperationException("Missing connection string 'docnest-db'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory")));
        
        services.Configure<FileStoreOptions>(configuration.GetSection("FileStore"));
        services.AddScoped<IFileStore, LocalFileStore>();
        
        return services;
    }
}