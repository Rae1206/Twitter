using Application.Interfaces.Services;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Controlador de autenticación.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Inicia sesión con las credenciales proporcionadas.
    /// </summary>
    [HttpPost("login")]
    [EndpointSummary("Inicia sesión como usuario")]
    [EndpointDescription("Este endpoint permite al usuario iniciar sesión en el sistema utilizando sus credenciales de usuario y contraseña." +
                         "Genera 2 tokens uno de JWT para la autentificación y otro para el refresco de tokens.")]
    [ProducesResponseType<GenericResponse<string>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<GenericResponse<LoginResponse>>(StatusCodes.Status200OK)]
    public IActionResult Login([FromBody] LoginUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = authService.Login(model);
        return Ok(response);
    }
}
