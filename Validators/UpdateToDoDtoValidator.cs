using FluentValidation;
using core_ledger_api.Dtos;

namespace core_ledger_api.Validators;

public class UpdateToDoDtoValidator : AbstractValidator<UpdateToDoDto>
{
    public UpdateToDoDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}
