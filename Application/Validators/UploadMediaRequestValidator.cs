using Application.Models.Requests.Media;
using FluentValidation;
using Shared.Constants;

namespace Application.Validators;

public class UploadMediaRequestValidator : AbstractValidator<UploadMediaRequest>
{
    public UploadMediaRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("El nombre de archivo es requerido");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("El archivo está vacío")
            .LessThanOrEqualTo((long)MediaConstants.MaxVideoSizeMb * 1024 * 1024)
            .WithMessage($"El archivo excede el tamaño máximo permitido de {MediaConstants.MaxVideoSizeMb} MB");
    }
}
