using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Interfaces.Services;

namespace Infrastructure.Persistence.Storage;

/// <summary>
/// S3-compatible storage implementation for DigitalOcean Spaces.
/// Stores media objects in a Spaces bucket and generates public CDN URLs.
/// </summary>
public class SpacesStorageService : IMediaStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;
    private readonly ILogger<SpacesStorageService> _logger;

    public SpacesStorageService(IConfiguration configuration, ILogger<SpacesStorageService> logger)
    {
        _logger = logger;

        var endpoint = configuration["Storage:Spaces:Endpoint"]
            ?? throw new InvalidOperationException("Storage:Spaces:Endpoint is required for DigitalOcean Spaces provider");

        _bucket = configuration["Storage:Spaces:Bucket"]
            ?? throw new InvalidOperationException("Storage:Spaces:Bucket is required for DigitalOcean Spaces provider");

        _publicBaseUrl = configuration["Storage:Spaces:PublicBaseUrl"]
            ?? throw new InvalidOperationException("Storage:Spaces:PublicBaseUrl is required for DigitalOcean Spaces provider");

        var accessKey = configuration["Storage:Spaces:AccessKey"]
            ?? throw new InvalidOperationException("Storage:Spaces:AccessKey is required for DigitalOcean Spaces provider");

        var secretKey = configuration["Storage:Spaces:SecretKey"]
            ?? throw new InvalidOperationException("Storage:Spaces:SecretKey is required for DigitalOcean Spaces provider");

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            // DigitalOcean Spaces is S3-compatible but does not require AWS signature v4 region resolution.
            // Using a minimal config with ServiceURL is sufficient.
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string mediaTypeFolder)
    {
        var objectKey = GenerateRelativePath(mediaTypeFolder, fileName);

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            InputStream = fileStream,
            ContentType = InferContentType(fileName),
            CannedACL = S3CannedACL.PublicRead
        };

        var response = await _s3Client.PutObjectAsync(putRequest);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Failed to upload file to Spaces. HTTP status: {response.HttpStatusCode}");
        }

        _logger.LogInformation("Uploaded object to Spaces: {Bucket}/{Key}", _bucket, objectKey);
        return objectKey;
    }

    public async Task<Stream> GetFileStreamAsync(string storagePath)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _bucket,
            Key = storagePath
        };

        var response = await _s3Client.GetObjectAsync(getRequest);

        // Copy S3 stream to a MemoryStream so the caller can dispose independently.
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteAsync(string storagePath)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = storagePath
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
        _logger.LogInformation("Deleted object from Spaces: {Bucket}/{Key}", _bucket, storagePath);
    }

    public string GenerateRelativePath(string mediaTypeFolder, string fileName)
    {
        var now = DateTime.UtcNow;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var fileId = Guid.NewGuid();
        return $"{mediaTypeFolder}/{now.Year}/{now.Month:D2}/{fileId}{ext}";
    }

    public Task<string> GetPublicUrlAsync(string storagePath, Guid? mediaId = null)
    {
        var url = $"{_publicBaseUrl.TrimEnd('/')}/{storagePath}";
        return Task.FromResult(url);
    }

    private static string InferContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".webm" => "video/webm",
            ".m4v" => "video/x-m4v",
            _ => "application/octet-stream"
        };
    }
}
