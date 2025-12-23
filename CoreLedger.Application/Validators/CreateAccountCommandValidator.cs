using FluentValidation;
using CoreLedger.Application.UseCases.Accounts.Commands;

namespace CoreLedger.Application.Validators;

/// <summary>
/// Validator for CreateAccountCommand.
/// </summary>
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Code)
            .GreaterThan(0)
            .WithMessage("Code must be a positive number")
            .LessThanOrEqualTo(9999999999)
            .WithMessage("Code cannot exceed 10 digits");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.TypeId)
            .GreaterThan(0)
            .WithMessage("TypeId must be a valid positive identifier");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid AccountStatus value");

        RuleFor(x => x.NormalBalance)
            .IsInEnum()
            .WithMessage("NormalBalance must be a valid NormalBalance value");
    }
}
