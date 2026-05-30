using Application.Interfaces.Services;
using Application.Models.Requests.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers;
using Twitter.Domain.Interfaces.Services;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para subir y obtener archivos multimedia.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Multimedia")]
public class MediaController(IMediaService mediaService, IMediaStorageService storageService) : ApiControllerBase
{
    [Authorize]
    [HttpPost("upload")]
    [EndpointSummary("Subir archivo multimedia")]
    [EndpointDescription("Permite subir un archivo de imagen o video al servidor.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [EndpointSummary("Obtener archivo multimedia por ID")]
    [EndpointDescription("Obtiene o redirige al archivo multimedia solicitado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedia(Guid id)
    {
        var media = await mediaService.GetByIdAsync(id);
        if (media == null)
        {
            return NotFoundEnvelope("Archivo no encontrado");
        }

        // Si la URL es absoluta (CDN externa como Spaces), redirigimos para evitar hacer proxy de archivos grandes
        if (Uri.IsWellFormedUriString(media.Url, UriKind.Absolute))
        {
            return Redirect(media.Url);
        }

        var stream = await storageService.GetFileStreamAsync(media.StoragePath);
        return File(stream, MediaContentTypeHelper.InferFromFileName(media.FileName), media.FileName);
    }
}
