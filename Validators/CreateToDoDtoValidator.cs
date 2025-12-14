using FluentValidation;
using core_ledger_api.Dtos;

namespace core_ledger_api.Validators;

public class CreateToDoDtoValidator : AbstractValidator<CreateToDoDto>
{
    public CreateToDoDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}
