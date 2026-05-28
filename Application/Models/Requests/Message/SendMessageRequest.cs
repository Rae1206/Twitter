using System.ComponentModel.DataAnnotations;

namespace Application.Models.Requests.Message;

public class SendMessageRequest
{
    [Required(ErrorMessage = "El ID del receptor es requerido")]
    public Guid ReceiverId { get; set; }

    [Required(ErrorMessage = "El contenido del mensaje es requerido")]
    [StringLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres")]
    [MinLength(1, ErrorMessage = "El mensaje no puede estar vacío")]
    public string Content { get; set; } = null!;
}
