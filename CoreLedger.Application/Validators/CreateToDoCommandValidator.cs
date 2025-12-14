using FluentValidation;
using CoreLedger.Application.UseCases.ToDos.Commands;

namespace CoreLedger.Application.Validators;

/// <summary>
/// Validator for CreateToDoCommand.
/// </summary>
public class CreateToDoCommandValidator : AbstractValidator<CreateToDoCommand>
{
    public CreateToDoCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}
