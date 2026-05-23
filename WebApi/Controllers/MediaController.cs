using Application.Interfaces.Services;
using Application.Models.Requests.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers;
using Twitter.Domain.Interfaces.Services;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaController(IMediaService mediaService, IMediaStorageService storageService) : ApiControllerBase
{
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedia([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequestEnvelope("No se proporcionó un archivo");
        }

        var userId = GetRequiredCurrentUserId();
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
        return File(stream, MediaContentTypeHelper.InferFromFileName(media.FileName), media.FileName);
    }
}
