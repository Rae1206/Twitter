namespace Application.Models.Responses.Auth;

/// <summary>
/// Response para solicitud de recuperación de contraseña.
/// </summary>
public class ResetPasswordResponse
{
    public bool EmailSent { get; set; }
}