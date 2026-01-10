using CoreLedger.Application.UseCases.Calendario.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateCalendarioCommand.
/// </summary>
public class UpdateCalendarioCommandValidator : AbstractValidator<UpdateCalendarioCommand>
{
    public UpdateCalendarioCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");

        RuleFor(x => x.TipoDia)
            .IsInEnum().WithMessage("TipoDia must be a valid enum value");

        RuleFor(x => x.Descricao)
            .MaximumLength(100).WithMessage("Descricao cannot exceed 100 characters");
    }
}
