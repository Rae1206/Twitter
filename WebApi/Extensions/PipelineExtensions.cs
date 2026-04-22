using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;

namespace WebApi.Extensions;

public static class PipelineExtensions
{
    public static void ConfigurePipeline(this WebApplication app)
    {
        // CORS primero
        app.UseCors();
        
        // Pipeline de middleware
        app.UseErrorHandler();
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