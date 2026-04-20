using System.ComponentModel.DataAnnotations;

namespace Application.Models.Requests.Auth;

/// <summary>
/// Request para verificar OTP y recuperar contraseña.
/// </summary>
public class VerifyOtpRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}