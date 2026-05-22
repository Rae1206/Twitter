using Twitter.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Storage;

public class LocalFileStorageService : IMediaStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["Storage:BasePath"];
        _basePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Path.GetTempPath(), "twitter-media")
            : configuredPath;

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public Task<string> SaveAsync(Stream fileStream, string fileName, string mediaTypeFolder)
    {
        var relativePath = GenerateRelativePath(mediaTypeFolder, fileName);
        var fullPath = Path.Combine(_basePath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStreamOut = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        fileStream.CopyTo(fileStreamOut);

        return Task.FromResult(relativePath);
    }

    public Task<Stream> GetFileStreamAsync(string storagePath)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Media file not found", fullPath);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string storagePath)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GenerateRelativePath(string mediaTypeFolder, string fileName)
    {
        var now = DateTime.UtcNow;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var fileId = Guid.NewGuid();
        return Path.Combine(mediaTypeFolder, now.Year.ToString(), now.Month.ToString("D2"), $"{fileId}{ext}").Replace('\\', '/');
    }

    public Task<string> GetPublicUrlAsync(string storagePath, Guid? mediaId = null)
    {
        if (mediaId is null)
        {
            throw new ArgumentException("mediaId is required for local file public URL generation", nameof(mediaId));
        }

        return Task.FromResult($"/api/media/{mediaId}");
    }
}
