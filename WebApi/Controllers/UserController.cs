using Application.Interfaces.Services;
using Application.Models.Requests.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Twitter.WebApi.Atributos;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[DeveloperAuthor(Name = "ALEX", Description = "Controller fo users")]
[ApiController]

public class UserController(IUserService userService, IEmailService emailService) : ApiControllerBase
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

    [HttpPut("{id:guid}/update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest model, Guid id)
    {
        var user = await userService.Update(id, model);
        return OkEnvelope(user);
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
}
