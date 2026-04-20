using System.ComponentModel.DataAnnotations;

namespace Application.Models.Requests.Auth;

/// <summary>
/// Request para solicitar recuperación de contraseña.
/// </summary>
public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}