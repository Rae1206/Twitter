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
    private readonly string _serviceUrl;
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

        var endpointUri = NormalizeEndpoint(endpoint, _bucket);

        _serviceUrl = endpointUri.GetLeftPart(UriPartial.Authority);
        _publicBaseUrl = NormalizePublicBaseUrl(_publicBaseUrl);

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _serviceUrl,
            UseHttp = endpointUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);

        _logger.LogInformation(
            "Configured DigitalOcean Spaces client with endpoint {Endpoint}, bucket {Bucket}, use HTTP {UseHttp}",
            _serviceUrl,
            _bucket,
            s3Config.UseHttp);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string mediaTypeFolder)
    {
        var objectKey = GenerateRelativePath(mediaTypeFolder, fileName);
        var contentType = InferContentType(fileName);

        _logger.LogInformation(
            "Uploading media to Spaces bucket {Bucket} using endpoint {Endpoint}. Key: {Key}, ContentType: {ContentType}",
            _bucket,
            _serviceUrl,
            objectKey,
            contentType);

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            InputStream = fileStream,
            ContentType = contentType
        };

        PutObjectResponse response;
        try
        {
            response = await _s3Client.PutObjectAsync(putRequest);
        }
        catch (AmazonS3Exception ex)
        {
            LogSpacesError(ex, "upload", objectKey);
            throw;
        }

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Failed to upload file to Spaces. HTTP status: {response.HttpStatusCode}");
        }

        _logger.LogInformation("Uploaded object to Spaces: {Bucket}/{Key}", _bucket, objectKey);
        return objectKey;
    }

    public async Task<Stream> GetFileStreamAsync(string storagePath)
    {
        _logger.LogInformation(
            "Downloading media from Spaces bucket {Bucket} using endpoint {Endpoint}. Key: {Key}",
            _bucket,
            _serviceUrl,
            storagePath);

        var getRequest = new GetObjectRequest
        {
            BucketName = _bucket,
            Key = storagePath
        };

        GetObjectResponse response;
        try
        {
            response = await _s3Client.GetObjectAsync(getRequest);
        }
        catch (AmazonS3Exception ex)
        {
            LogSpacesError(ex, "download", storagePath);
            throw;
        }

        // Copy S3 stream to a MemoryStream so the caller can dispose independently.
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteAsync(string storagePath)
    {
        _logger.LogInformation(
            "Deleting media from Spaces bucket {Bucket} using endpoint {Endpoint}. Key: {Key}",
            _bucket,
            _serviceUrl,
            storagePath);

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = storagePath
        };

        try
        {
            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
        catch (AmazonS3Exception ex)
        {
            LogSpacesError(ex, "delete", storagePath);
            throw;
        }

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

    private void LogSpacesError(AmazonS3Exception ex, string operation, string objectKey)
    {
        _logger.LogError(
            ex,
            "Spaces {Operation} failed. Endpoint: {Endpoint}, Bucket: {Bucket}, Key: {Key}, StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, RequestId: {RequestId}, HostId: {HostId}",
            operation,
            _serviceUrl,
            _bucket,
            objectKey,
            ex.StatusCode,
            ex.ErrorCode,
            ex.RequestId,
            ex.AmazonId2);
    }

    private static Uri NormalizeEndpoint(string endpoint, string bucket)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new InvalidOperationException("Storage:Spaces:Endpoint must be a valid absolute URL");
        }

        var normalizedHost = endpointUri.Host;
        var bucketPrefix = $"{bucket}.";

        if (normalizedHost.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase)
            && normalizedHost.Contains(".digitaloceanspaces.com", StringComparison.OrdinalIgnoreCase))
        {
            normalizedHost = normalizedHost[bucketPrefix.Length..];
        }

        return new UriBuilder(endpointUri.Scheme, normalizedHost, endpointUri.IsDefaultPort ? -1 : endpointUri.Port).Uri;
    }

    private static string NormalizePublicBaseUrl(string publicBaseUrl)
    {
        if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicBaseUri))
        {
            throw new InvalidOperationException("Storage:Spaces:PublicBaseUrl must be a valid absolute URL");
        }

        return publicBaseUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
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
