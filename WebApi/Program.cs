using Microsoft.AspNetCore.Mvc;
using Serilog;
using WebApi.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using WebApi.Common;
using Microsoft.AspNetCore.SignalR;

// Logger de arranque para errores durante el inicio.
// Solo escribe a consola hasta que UseSerilog lo reemplaza con la configuración completa.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando Twitter Web API");

    var builder = WebApplication.CreateBuilder(args);

    // Cargar configuración de secret.json
    builder.Configuration.AddJsonFile("secret.json", optional: true, reloadOnChange: true);

// Configuración infraestructura
    builder.Services.AddSignalR(options =>
    {
        // Habilitar detección de cambios automática para que los
        // recibos de lectura se propaguen sin refresh manual.
        options.EnableDetailedErrors = true;
    })
    .AddJsonProtocol(options =>
    {
        // Forzar camelCase en los payloads de SignalR para que coincida
        // con el contracto que espera el frontend (MessageReadReceipt, MessageDto, etc.).
        // Sin esto, SignalR serializa en PascalCase (.NET default) y el cliente
        // no puede mapear las propiedades (messageId vs MessageId).
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
    builder.Services.AddSingleton<IUserIdProvider, WebApi.Hubs.UserIdProvider>();
    builder.ConfigureSerilog();
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context => ApiResponseFactory.Validation(context.ModelState);
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    
    // CORS para Scalar y frontend (SignalR requiere configuración especial)
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
        
        // Política específica para SignalR - permite cualquier origen con credenciales
        options.AddPolicy("SignalRPolicy", policy =>
        {
            policy.SetIsOriginAllowed(_ => true) // Permite cualquier origen
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // SignalR requiere credenciales
        });
    });

    // TODA la infraestructura consolidada (DbContext, Cache, Repositorios, Servicios, JWT)
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Configurar pipeline
    app.ConfigurePipeline();

    // Crear usuario administrador por defecto si no existe
    app.SeedDefaultAdmin();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó de forma inesperada");
}
finally
{
    Log.CloseAndFlush();
}
