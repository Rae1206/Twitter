using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Twitter.Domain.Database.SqlServer.Context;

namespace Twitter.Domain.Database.SqlServer;

public class TwitterDbContextFactory : IDesignTimeDbContextFactory<TwitterDbContext>
{
    public TwitterDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var webApiPath = ResolveWebApiPath();
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(webApiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddJsonFile("secret.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        TryAddUserSecrets(configurationBuilder);

        var configuration = configurationBuilder.Build();
        var connectionString = FirstNonEmpty(
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"),
            configuration.GetConnectionString("DefaultConnection"),
            Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE"))
            ?? throw new InvalidOperationException("No SQL Server connection string could be resolved for design-time migrations.");

        var optionsBuilder = new DbContextOptionsBuilder<TwitterDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TwitterDbContext(optionsBuilder.Options);
    }

    private static string ResolveWebApiPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.Combine(currentDirectory, "WebApi"),
            Path.Combine(currentDirectory, "..", "WebApi"),
            Path.Combine(AppContext.BaseDirectory, "WebApi"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "WebApi")
        };

        foreach (var candidate in candidates.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not locate the WebApi project directory for design-time configuration.");
    }

    private static void TryAddUserSecrets(IConfigurationBuilder configurationBuilder)
    {
        try
        {
            var webApiAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "WebApi")
                ?? System.Reflection.Assembly.Load("WebApi");

            configurationBuilder.AddUserSecrets(webApiAssembly, optional: true);
        }
        catch
        {
            // Ignore when the startup assembly or user secrets metadata is unavailable.
        }
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
