using CoreLedger.Application.UseCases.HistoricosIndexadores.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateHistoricoIndexadorCommand with business validation rules.
/// </summary>
public class CreateHistoricoIndexadorCommandValidator : AbstractValidator<CreateHistoricoIndexadorCommand>
{
    public CreateHistoricoIndexadorCommandValidator()
    {
        RuleFor(x => x.IndexadorId)
            .GreaterThan(0)
            .WithMessage("IndexadorId must be a valid positive identifier");

        RuleFor(x => x.DataReferencia)
            .NotEmpty()
            .WithMessage("DataReferencia is required");

        RuleFor(x => x.Valor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor must be greater than or equal to zero");

        RuleFor(x => x.FatorDiario)
            .GreaterThan(0)
            .When(x => x.FatorDiario.HasValue)
            .WithMessage("Fator diário must be greater than zero when provided");

        RuleFor(x => x.VariacaoPercentual)
            .Must(v => !v.HasValue || v.Value >= -100)
            .WithMessage("Variação percentual must be greater than or equal to -100% when provided");

        RuleFor(x => x.Fonte)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Fonte))
            .WithMessage("Fonte cannot exceed 50 characters");
    }
}
