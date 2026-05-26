using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;
using WebApi.Hubs;

namespace WebApi.Extensions;

public static class PipelineExtensions
{
public static void ConfigurePipeline(this WebApplication app)
    {
        // Necesario detrás de proxy (Render/Cloudflare) para respetar https real
        app.UseForwardedHeaders();

        // CORS primero - usar política específica para SignalR
        app.UseCors("SignalRPolicy");
        
        // Pipeline de middleware - sin HTTPS redirect en producción
        app.UseErrorHandler();
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        
        // Registrar controllers
        app.MapControllers();

        // Mapear el Hub de SignalR
        app.MapHub<MessageHub>("/hubs/message");

        // OpenAPI 
        app.MapOpenApi();

        // Scalar UI
        app.MapScalarApiReference();
    }
}
