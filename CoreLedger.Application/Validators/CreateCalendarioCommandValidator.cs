using CoreLedger.Application.UseCases.Calendario.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateCalendarioCommand.
/// </summary>
public class CreateCalendarioCommandValidator : AbstractValidator<CreateCalendarioCommand>
{
    public CreateCalendarioCommandValidator()
    {
        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Data is required")
            .Must(BeValidDate).WithMessage("Data must be a valid date between 1900 and 2100");

        RuleFor(x => x.TipoDia)
            .IsInEnum().WithMessage("TipoDia must be a valid enum value");

        RuleFor(x => x.Praca)
            .IsInEnum().WithMessage("Praca must be a valid enum value");

        RuleFor(x => x.Descricao)
            .MaximumLength(100).WithMessage("Descricao cannot exceed 100 characters");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("CreatedByUserId is required")
            .MaximumLength(200).WithMessage("CreatedByUserId cannot exceed 200 characters");
    }

    private bool BeValidDate(DateOnly data)
    {
        return data != default && data.Year >= 1900 && data.Year <= 2100;
    }
}
