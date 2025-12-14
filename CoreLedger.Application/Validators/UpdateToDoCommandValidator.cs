using FluentValidation;
using CoreLedger.Application.UseCases.ToDos.Commands;

namespace CoreLedger.Application.Validators;

/// <summary>
/// Validator for UpdateToDoCommand.
/// </summary>
public class UpdateToDoCommandValidator : AbstractValidator<UpdateToDoCommand>
{
    public UpdateToDoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}
