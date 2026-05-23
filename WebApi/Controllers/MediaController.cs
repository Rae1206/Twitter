using Application.Interfaces.Services;
using Application.Models.Requests.Media;
using Microsoft.AspNetCore.Mvc;
using Twitter.Domain.Interfaces.Services;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaController(IMediaService mediaService, IMediaStorageService storageService) : ApiControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequestEnvelope("No se proporcionó un archivo");
        }

        var userId = TryGetCurrentUserId() ?? Guid.Empty;
        using var stream = file.OpenReadStream();
        var request = new UploadMediaRequest
        {
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
        var result = await mediaService.UploadAsync(request, userId);
        return OkEnvelope(result, "Archivo subido correctamente");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMedia(Guid id)
    {
        var media = await mediaService.GetByIdAsync(id);
        if (media == null)
        {
            return NotFoundEnvelope("Archivo no encontrado");
        }

        // If the media URL is absolute (external CDN like Spaces), redirect to avoid proxying large files.
        if (Uri.IsWellFormedUriString(media.Url, UriKind.Absolute))
        {
            return Redirect(media.Url);
        }

        var stream = await storageService.GetFileStreamAsync(media.StoragePath);
        return File(stream, GetContentType(media.FileName), media.FileName);
    }

    private static string GetContentType(string fileName)
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
