using Application.Interfaces.Services;
using Application.Models.Requests.Auth;
using Application.Models.Responses.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Controlador de autenticación.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Autenticación")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("login")]
    [EndpointSummary("Inicia sesión como usuario")]
    [EndpointDescription("Este endpoint permite al usuario iniciar sesión en el sistema utilizando sus credenciales de usuario y contraseña. Genera un token JWT (1-5 min) y un refresh token (15 días).")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginAuthRequest model)
    {
        var response = await authService.Login(model);
        return OkEnvelope(response);
    }

    [HttpPost("renew")]
    [EndpointSummary("Renovar token de acceso")]
    [EndpointDescription("Este endpoint permite renovar el token de acceso usando un refresh token válido. Devuelve un nuevo token JWT y un nuevo refresh token.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Renew([FromBody] RenewAuthRequest model)
    {
        var response = await authService.Renew(model);
        return OkEnvelope(response);
    }

    [HttpPost("reset-password")]
    [EndpointSummary("Solicitar recuperación de contraseña")]
    [EndpointDescription("Envía un código OTP al correo del usuario para recuperar la contraseña.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
    {
        var response = await authService.RequestPasswordReset(model);
        return OkEnvelope(response);
    }

    [HttpPost("verify-otp")]
    [EndpointSummary("Verificar OTP y cambiar contraseña")]
    [EndpointDescription("Verifica el código OTP enviado por email y cambia la contraseña.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest model)
    {
        var response = await authService.VerifyOtpAndResetPassword(model);
        return OkEnvelope(response);
    }
}
