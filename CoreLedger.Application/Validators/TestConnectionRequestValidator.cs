using FluentValidation;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.Validators;

/// <summary>
/// Validator for TestConnectionRequest DTO.
/// </summary>
public class TestConnectionRequestValidator : AbstractValidator<TestConnectionRequest>
{
    public TestConnectionRequestValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .WithMessage("ReferenceId is required")
            .MaximumLength(50)
            .WithMessage("ReferenceId cannot exceed 50 characters");

        RuleFor(x => x.JobDescription)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.JobDescription))
            .WithMessage("JobDescription cannot exceed 255 characters");
    }
}
