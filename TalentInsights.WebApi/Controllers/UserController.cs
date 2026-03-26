using Microsoft.AspNetCore.Mvc;
using TalentInsights.Application.Interfaces.Services;
using TalentInsights.Application.Models.Requests.User;

namespace TalentInsights.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest model)
    {
        var rsp = userService.Create(model);
        return Ok(rsp);
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] GetAllUserRequest model)
    {
        var rsp = userService.Get(model.Limit ?? 0, model.Offset ?? 0);
        return Ok(rsp);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var rsp = userService.Get(id);
        return Ok(rsp);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update([FromBody] UpdateUserRequest model, Guid id)
    {
        var rsp = userService.Update(id, model);
        return Ok(rsp);
    }

    [HttpPatch("change-password/{id:guid}")]
    public IActionResult ChangePassword(Guid id, [FromBody] ChangePasswordUserRequest model)
    {
        var rsp = userService.ChangePassword(id, model);
        return Ok(rsp);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var rsp = userService.Delete(id);
        return Ok(rsp);
    }
}
