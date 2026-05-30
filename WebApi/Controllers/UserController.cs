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
    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="model">Modelo que contiene los datos del nuevo usuario a registrar.</param>
    /// <returns>Los detalles del usuario creado.</returns>
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

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros opcionales de nickname y correo.
    /// </summary>
    /// <param name="model">Modelo de consulta con los filtros y límites de paginación.</param>
    /// <returns>La lista paginada de usuarios.</returns>
    [HttpGet("list")]
    [EndpointSummary("Listar usuarios")]
    [EndpointDescription("Obtiene una lista paginada de usuarios con filtros opcionales.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUserRequest model)
    {
        var rsp = await userService.Get(model.Limit ?? 0, model.Offset ?? 0, model.Nickname, model.Email);
        return OkEnvelope(rsp);
    }

    /// <summary>
    /// Obtiene la información detallada de un usuario por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del usuario a consultar.</param>
    /// <returns>Los detalles del usuario solicitado.</returns>
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

    /// <summary>
    /// Actualiza los datos del perfil (apodo, biografía, etc.) del usuario autenticado actualmente.
    /// </summary>
    /// <param name="model">Modelo con la información de perfil actualizada.</param>
    /// <returns>Los detalles actualizados del usuario.</returns>
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

    /// <summary>
    /// Sube o reemplaza la foto de perfil del usuario autenticado actualmente.
    /// </summary>
    /// <param name="file">El archivo de imagen enviado en el cuerpo de la petición (multipart/form-data).</param>
    /// <returns>Los detalles actualizados de la foto de perfil del usuario.</returns>
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

    /// <summary>
    /// Permite al usuario autenticado cambiar su contraseña actual.
    /// </summary>
    /// <param name="model">Modelo de solicitud con la contraseña actual y la nueva contraseña.</param>
    /// <returns>Una respuesta indicando el éxito del cambio de contraseña.</returns>
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

    /// <summary>
    /// Elimina un usuario de forma lógica en el sistema. Requiere permiso de administrador para eliminar.
    /// </summary>
    /// <param name="id">Identificador único del usuario a eliminar.</param>
    /// <returns>Una respuesta indicando el éxito de la eliminación.</returns>
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

    /// <summary>
    /// Obtiene la información de perfil completa del usuario autenticado actualmente.
    /// </summary>
    /// <returns>La información de perfil del usuario actual.</returns>
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

    /// <summary>
    /// Obtiene y retorna la foto de perfil de un usuario determinado, o redirige si es externa.
    /// </summary>
    /// <param name="id">Identificador único del usuario.</param>
    /// <returns>El flujo del archivo de imagen o una redirección.</returns>
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
            var stream = mediaStorageService.GetFileStreamAsync(userPhoto.StoragePath!);
            return File(await stream, MediaContentTypeHelper.InferFromFileName(userPhoto.FileName!));
        }
        catch (FileNotFoundException)
        {
            return NotFoundEnvelope("Foto de perfil no encontrada");
        }
    }
}
