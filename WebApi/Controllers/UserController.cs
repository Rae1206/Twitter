using Application.Interfaces.Services;
using Application.Models.Requests.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] CreateUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = userService.Create(model);
        return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
    }

    [Authorize(Roles = "Admin")]
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
    public IActionResult UpdateUser([FromBody] UpdateUserRequest model, Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = userService.Update(id, model);
        return Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/change-password")]
    public IActionResult ChangeUserPassword(Guid id, [FromBody] ChangePasswordUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        userService.ChangePassword(id, model);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/delete")]
    public IActionResult DeleteUser(Guid id)
    {
        userService.Delete(id);
        return NoContent();
    }
}
