using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Helpers;
using Twitter.Domain.Interfaces;

namespace Infrastructure.Background;

/// <summary>
/// Soft-deletea posts efímeros vencidos. El filtro global de TwitterDbContext ya los oculta del feed
/// en tiempo real, por lo que este servicio solo cierra el ciclo: marca DeletedAt para que aparezcan
/// como eliminados en el panel admin (con DeletedByAdminId = null para distinguirlos de un delete admin).
///
/// Corre cada 5 minutos. Procesa hasta 500 posts por ciclo para evitar locks largos en tablas grandes.
/// </summary>
public class EphemeralPostCleanupService : IHostedService, IDisposable
{
    private const int BatchSize = 500;
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EphemeralPostCleanupService> _logger;
    private Timer? _timer;

    public EphemeralPostCleanupService(
        IServiceProvider serviceProvider,
        ILogger<EphemeralPostCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ephemeral post cleanup service started (interval: {Interval})", Interval);
        _timer = new Timer(DoCleanup, null, InitialDelay, Interval);
        return Task.CompletedTask;
    }

    private async void DoCleanup(object? state)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Damos un margen sobre ExpiresAt antes de soft-deletar, para evitar carreras con clientes
            // que estén renderizando el countdown final.
            var cutoff = DateTimeHelper.UtcNow().AddMinutes(-PostConstants.EphemeralCleanupGraceMinutes);
            var expired = await unitOfWork.Posts.GetExpiredPendingSoftDeleteAsync(cutoff, BatchSize);

            if (expired.Count == 0)
            {
                return;
            }

            foreach (var post in expired)
            {
                post.DeletedAt = DateTimeHelper.UtcNow();
                post.DeletedReason = "Ephemeral post expired";
                unitOfWork.Update(post);
            }

            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Ephemeral post cleanup: soft-deleted {Count} expired posts", expired.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ephemeral post cleanup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _logger.LogInformation("Ephemeral post cleanup service stopped");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
