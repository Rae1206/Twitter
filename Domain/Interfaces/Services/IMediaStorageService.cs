namespace Twitter.Domain.Interfaces.Services;

public interface IMediaStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, string mediaTypeFolder);
    Task<Stream> GetFileStreamAsync(string storagePath);
    Task DeleteAsync(string storagePath);
    string GenerateRelativePath(string mediaTypeFolder, string fileName);
    Task<string> GetPublicUrlAsync(string storagePath, Guid? mediaId = null);
}
