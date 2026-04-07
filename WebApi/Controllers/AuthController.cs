using Application.Interfaces.Services;
using Application.Models.Requests.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var secretKey = configuration["Jwt:SecretKey"];
        var response = authService.Login(model, secretKey!);
        return Ok(response);
    }
}
