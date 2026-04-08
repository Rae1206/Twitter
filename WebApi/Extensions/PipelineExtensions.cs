using Scalar.AspNetCore;

namespace WebApi.Extensions;

public static class PipelineExtensions
{
    public static void ConfigurePipeline(this WebApplication app)
    {
        // Endpoints solo para desarrollo
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Documentación de Twitter API");
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                options.Theme = ScalarTheme.Purple;
            });
        }

        // Pipeline de middleware
        app.UseErrorHandler();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // Endpoints
        app.MapControllers();
    }
}
