using Application.Interfaces.Services;
using Application.Models.Requests.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Twitter.WebApi.Atributos;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[DeveloperAuthor(Name = "ALEX", Description = "Controller fo users")]
[ApiController]

public class UserController(IUserService userService, IEmailService emailService) : ControllerBase
{
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromQuery] string to)
    {
        try
        {
            await emailService.SendWelcomeEmailAsync(to, "Test User");
            return Ok(new { message = "Email enviado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await userService.Create(model);
        return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
    }

   // [Authorize(Roles = "Admin")]
    [HttpGet("list")]
    public IActionResult GetAllUsers([FromQuery] GetAllUserRequest model)
    {
        var rsp = userService.Get(model.Limit ?? 0, model.Offset ?? 0, model.FullName, model.Email);
        return Ok(rsp);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetUserById(Guid id)
    {
        var user = userService.Get(id);
        return Ok(user);
    }

    [HttpPut("{id:guid}/update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest model, Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await userService.Update(id, model);
        return Ok(user);
    }

    [Authorize]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimsConstants.USER_ID)?.Value
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);

        var userId = Guid.Parse(userIdClaim);
        await userService.ChangePassword(userId, model);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/delete")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await userService.Delete(id);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimsConstants.USER_ID)?.Value
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);

        var userId = Guid.Parse(userIdClaim);
        var user = userService.Get(userId);
        return Ok(user);
    }
}
