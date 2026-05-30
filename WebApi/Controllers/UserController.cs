using Application.Interfaces.Services;
using Application.Models.Requests.Media;
using Application.Models.Requests.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Helpers;
using Twitter.Domain.Interfaces.Services;
using WebApi.Attributes;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para gestionar usuarios y sus perfiles.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Usuarios")]
public class UserController(
    IUserService userService,
    IAvatarService avatarService,
    IMediaStorageService mediaStorageService) : ApiControllerBase
{
    [HttpPost("create")]
    [EndpointSummary("Registrar usuario")]
    [EndpointDescription("Crea un nuevo usuario en el sistema.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest model)
    {
        var user = await userService.Create(model);
        return CreatedEnvelope(nameof(GetUserById), new { id = user.UserId }, user);
    }

    [HttpGet("list")]
    [EndpointSummary("Listar usuarios")]
    [EndpointDescription("Obtiene una lista paginada de usuarios con filtros opcionales.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUserRequest model)
    {
        var rsp = await userService.Get(model.Limit ?? 0, model.Offset ?? 0, model.Nickname, model.Email);
        return OkEnvelope(rsp);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Obtener usuario por ID")]
    [EndpointDescription("Obtiene la información de un usuario por su identificador.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await userService.Get(id);
        return OkEnvelope(user);
    }

    [Authorize]
    [HttpPut("me")]
    [EndpointSummary("Actualizar mi perfil")]
    [EndpointDescription("Actualiza los datos del perfil del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var user = await userService.UpdateProfile(userId, model);
        return OkEnvelope(user);
    }

    [Authorize]
    [HttpPost("me/avatar")]
    [EndpointSummary("Subir foto de perfil")]
    [EndpointDescription("Sube o reemplaza la foto de perfil del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadCurrentUserAvatar([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequestEnvelope("No se proporcionó una foto de perfil");
        }

        var userId = GetRequiredCurrentUserId();
        await using var stream = file.OpenReadStream();
        var request = new UploadMediaRequest
        {
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };

        var user = await avatarService.UploadProfilePhotoAsync(userId, request);
        return OkEnvelope(user, "Foto de perfil actualizada correctamente");
    }

    [Authorize]
    [HttpPatch("change-password")]
    [EndpointSummary("Cambiar contraseña")]
    [EndpointDescription("Cambia la contraseña del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordUserRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        await userService.ChangePassword(userId, model);
        return SuccessEnvelope("Contraseña actualizada correctamente");
    }

    [RequirePermission(PermissionConstants.UsersDelete)]
    [HttpDelete("{id:guid}/delete")]
    [EndpointSummary("Eliminar usuario")]
    [EndpointDescription("Elimina un usuario del sistema. Requiere permiso de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await userService.Delete(id);
        return SuccessEnvelope("Usuario eliminado correctamente");
    }

    [Authorize]
    [HttpGet("me")]
    [EndpointSummary("Obtener mi perfil")]
    [EndpointDescription("Obtiene la información del usuario autenticado actualmente.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetRequiredCurrentUserId();
        var user = await userService.Get(userId);
        return OkEnvelope(user);
    }

    [HttpGet("{id:guid}/avatar")]
    [EndpointSummary("Obtener foto de perfil")]
    [EndpointDescription("Obtiene la foto de perfil de un usuario. Redirige si es una URL externa.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserAvatar(Guid id)
    {
        var userPhoto = await avatarService.GetProfilePhotoAsync(id);
        var isAbsoluteUrl = Uri.IsWellFormedUriString(userPhoto.Url, UriKind.Absolute);

        if (string.IsNullOrWhiteSpace(userPhoto.Url)
            || (!isAbsoluteUrl && (string.IsNullOrWhiteSpace(userPhoto.FileName) || string.IsNullOrWhiteSpace(userPhoto.StoragePath))))
        {
            return NotFoundEnvelope("Foto de perfil no encontrada");
        }

        // Si es URL externa (CDN), redirige para no hacer proxy de archivos grandes
        if (isAbsoluteUrl)
        {
            return Redirect(userPhoto.Url);
        }

        try
        {
            var stream = await mediaStorageService.GetFileStreamAsync(userPhoto.StoragePath!);
            return File(stream, MediaContentTypeHelper.InferFromFileName(userPhoto.FileName!));
        }
        catch (FileNotFoundException)
        {
            return NotFoundEnvelope("Foto de perfil no encontrada");
        }
    }
}
