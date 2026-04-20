using Application.Interfaces.Services;
using Application.Models.Requests.Auth;
using Application.Models.Responses;
using Application.Models.Responses.Auth;
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
    /// <returns>Token JWT y refresh token</returns>
    [HttpPost("login")]
    [EndpointSummary("Inicia sesión como usuario")]
    [EndpointDescription("Este endpoint permite al usuario iniciar sesión en el sistema utilizando sus credenciales de usuario y contraseña. Genera un token JWT (1-5 min) y un refresh token (15 días).")]
    [ProducesResponseType<GenericResponse<LoginAuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginAuthRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = authService.Login(model);
        return Ok(response);
    }

    /// <summary>
    /// Renueva el token de acceso usando un refresh token válido.
    /// </summary>
    /// <returns>Nuevo token JWT y nuevo refresh token</returns>
    [HttpPost("renew")]
    [EndpointSummary("Renovar token de acceso")]
    [EndpointDescription("Este endpoint permite renovar el token de acceso usando un refresh token válido. Devuelve un nuevo token JWT y un nuevo refresh token.")]
    [ProducesResponseType<GenericResponse<LoginAuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Renew([FromBody] RenewAuthRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = authService.Renew(model);
        return Ok(response);
    }

    /// <summary>
    /// Solicita recuperación de contraseña - envía OTP por email.
    /// </summary>
    [HttpPost("reset-password")]
    [EndpointSummary("Solicitar recuperación de contraseña")]
    [EndpointDescription("Envía un código OTP al correo del usuario para recuperar la contraseña.")]
    [ProducesResponseType<GenericResponse<ResetPasswordResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await authService.RequestPasswordReset(model);
        return Ok(response);
    }

    /// <summary>
    /// Verifica OTP y cambia la contraseña.
    /// </summary>
    [HttpPost("verify-otp")]
    [EndpointSummary("Verificar OTP y cambiar contraseña")]
    [EndpointDescription("Verifica el código OTP enviado por email y cambia la contraseña.")]
    [ProducesResponseType<GenericResponse<LoginAuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await authService.VerifyOtpAndResetPassword(model);
        return Ok(response);
    }
}