using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;

namespace WebApi.Extensions;

public static class PipelineExtensions
{
public static void ConfigurePipeline(this WebApplication app)
    {
        // Necesario detrás de proxy (Render/Cloudflare) para respetar https real
        app.UseForwardedHeaders();

        // CORS primero
        app.UseCors();
        
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

        // OpenAPI 
        app.MapOpenApi();

        // Scalar UI
        app.MapScalarApiReference();
    }
}
