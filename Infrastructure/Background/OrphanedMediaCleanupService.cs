using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Interfaces.Services;

namespace Infrastructure.Background;

public class OrphanedMediaCleanupService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrphanedMediaCleanupService> _logger;
    private Timer? _timer;

    public OrphanedMediaCleanupService(
        IServiceProvider serviceProvider,
        ILogger<OrphanedMediaCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Orphaned media cleanup service started");
        _timer = new Timer(DoCleanup, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15));
        return Task.CompletedTask;
    }

    private async void DoCleanup(object? state)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var storageService = scope.ServiceProvider.GetRequiredService<IMediaStorageService>();

            var cutoff = DateTime.UtcNow.AddHours(-1);
            var orphans = await unitOfWork.PostMedias.GetOrphansOlderThanAsync(cutoff);

            foreach (var orphan in orphans)
            {
                try
                {
                    await storageService.DeleteAsync(orphan.StoragePath);
                    unitOfWork.Delete(orphan);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete orphaned media: {MediaId}", orphan.MediaId);
                }
            }

            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} orphaned media files", orphans.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during orphaned media cleanup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
