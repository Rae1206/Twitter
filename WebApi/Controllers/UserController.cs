using Application.Interfaces.Services;
using Application.Models.Requests.Media;
using Application.Models.Requests.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Helpers;
using Twitter.Domain.Interfaces.Services;
using Twitter.WebApi.Atributos;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[DeveloperAuthor(Name = "ALEX", Description = "Controller fo users")]
[ApiController]

public class UserController(
    IUserService userService,
    IEmailService emailService,
    IMediaStorageService mediaStorageService) : ApiControllerBase
{
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromQuery] string to)
    {
        try
        {
            await emailService.SendWelcomeEmailAsync(to, "Test User");
            return OkEnvelope(new { }, "Email enviado");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, WebApi.Common.ApiResponseFactory.Error("No se pudo enviar el email de prueba", [ex.Message]));
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest model)
    {
        var user = await userService.Create(model);
        return CreatedEnvelope(nameof(GetUserById), new { id = user.UserId }, user);
    }

   // [Authorize(Roles = "Admin")]
    [HttpGet("list")]
    public IActionResult GetAllUsers([FromQuery] GetAllUserRequest model)
    {
        var rsp = userService.Get(model.Limit ?? 0, model.Offset ?? 0, model.FullName, model.Email);
        return OkEnvelope(rsp);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetUserById(Guid id)
    {
        var user = userService.Get(id);
        return OkEnvelope(user);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var user = await userService.UpdateProfile(userId, model);
        return OkEnvelope(user);
    }

    [Authorize]
    [HttpPost("me/avatar")]
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

        var user = await userService.UploadProfilePhoto(userId, request);
        return OkEnvelope(user, "Foto de perfil actualizada correctamente");
    }

    [Authorize]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordUserRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        await userService.ChangePassword(userId, model);
        return SuccessEnvelope("Contraseña actualizada correctamente");
    }

    // [Authorize(Roles = "Admin")]
    [RequirePermission(PermissionConstants.UsersDelete)]
    [HttpDelete("{id:guid}/delete")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await userService.Delete(id);
        return SuccessEnvelope("Usuario eliminado correctamente");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = GetRequiredCurrentUserId();
        var user = userService.Get(userId);
        return OkEnvelope(user);
    }

    [HttpGet("{id:guid}/avatar")]
    public async Task<IActionResult> GetUserAvatar(Guid id)
    {
        var userPhoto = userService.GetProfilePhoto(id);
        var isAbsoluteUrl = Uri.IsWellFormedUriString(userPhoto.Url, UriKind.Absolute);

        if (string.IsNullOrWhiteSpace(userPhoto.Url)
            || (!isAbsoluteUrl && (string.IsNullOrWhiteSpace(userPhoto.FileName) || string.IsNullOrWhiteSpace(userPhoto.StoragePath))))
        {
            return NotFoundEnvelope("Foto de perfil no encontrada");
        }

        if (isAbsoluteUrl)
        {
            return Redirect(userPhoto.Url);
        }

        try
        {
            var stream = await mediaStorageService.GetFileStreamAsync(userPhoto.StoragePath!);
            return File(stream, MediaContentTypeHelper.InferFromFileName(userPhoto.FileName!), userPhoto.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFoundEnvelope("Foto de perfil no encontrada");
        }
    }
}
