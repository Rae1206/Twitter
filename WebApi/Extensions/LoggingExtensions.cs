using MongoDB.Driver;
using Serilog;
using Serilog.Sinks.MongoDB;
using Shared.Constants;

namespace WebApi.Extensions;

/// <summary>
/// Extensiones para configurar Serilog con MongoDB.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>Configura Serilog con escritura a consola y MongoDB (si está disponible).</summary>
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var mongoUrl = builder.Configuration.GetConnectionString(ConnectionStringNames.MongoDb);

        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Twitter.WebApi");

            if (!string.IsNullOrEmpty(mongoUrl))
            {
                var urlBuilder = new MongoUrlBuilder(mongoUrl)
                {
                    DatabaseName = "TwitterLogs",
                    ConnectTimeout = TimeSpan.FromSeconds(3),
                    ServerSelectionTimeout = TimeSpan.FromSeconds(3)
                };

                configuration.WriteTo.Async(async =>
                {
                    async.MongoDBBson(cfg =>
                    {
                        cfg.SetMongoUrl(urlBuilder.ToMongoUrl().ToString());
                        cfg.SetCollectionName("Logs");
                        cfg.SetCreateCappedCollection(1024);
                        cfg.SetExpireTTL(TimeSpan.FromDays(30));
                    });
                });
            }
        });
    }
}
