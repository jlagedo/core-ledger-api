using CoreLedger.Application.UseCases.Indexadores.Commands;
using CoreLedger.Domain.Enums;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for UpdateIndexadorCommand with business rules IDX-003, IDX-004.
/// </summary>
public class UpdateIndexadorCommandValidator : AbstractValidator<UpdateIndexadorCommand>
{
    public UpdateIndexadorCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be a valid positive identifier");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome is required")
            .MaximumLength(100)
            .WithMessage("Nome cannot exceed 100 characters");

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("Tipo must be a valid IndexadorTipo value");

        RuleFor(x => x.Periodicidade)
            .IsInEnum()
            .WithMessage("Periodicidade must be a valid Periodicidade value");

        RuleFor(x => x.Fonte)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Fonte))
            .WithMessage("Fonte cannot exceed 100 characters");

        // FatorAcumulado requires DataBase
        RuleFor(x => x.FatorAcumulado)
            .GreaterThan(0)
            .When(x => x.FatorAcumulado.HasValue)
            .WithMessage("Fator acumulado must be greater than zero");

        RuleFor(x => x.DataBase)
            .NotNull()
            .When(x => x.FatorAcumulado.HasValue)
            .WithMessage("Data base is required when fator acumulado is provided");

        // IDX-003: ImportacaoAutomatica requires UrlFonte
        RuleFor(x => x.UrlFonte)
            .NotEmpty()
            .When(x => x.ImportacaoAutomatica)
            .WithMessage("URL fonte é obrigatória quando importação automática está habilitada");

        RuleFor(x => x.UrlFonte)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.UrlFonte))
            .WithMessage("URL fonte cannot exceed 500 characters");

        // IDX-004: Periodicidade compatibility with Tipo
        RuleFor(x => x)
            .Custom((command, context) =>
            {
                if (!IsPeriodicidadeCompatibleWithTipo(command.Tipo, command.Periodicidade))
                {
                    context.AddFailure(
                        nameof(command.Periodicidade),
                        $"Periodicidade {command.Periodicidade} não é compatível com o tipo {command.Tipo}");
                }
            });
    }

    private static bool IsPeriodicidadeCompatibleWithTipo(IndexadorTipo tipo, Periodicidade periodicidade)
    {
        return tipo switch
        {
            IndexadorTipo.Juros => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.Inflacao => periodicidade == Periodicidade.Mensal ||
                                      periodicidade == Periodicidade.Anual,
            IndexadorTipo.Cambio => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.IndiceBolsa => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.IndiceRendaFixa => periodicidade == Periodicidade.Diaria ||
                                             periodicidade == Periodicidade.Mensal,
            IndexadorTipo.Crypto => periodicidade == Periodicidade.Diaria,
            IndexadorTipo.Outro => true, // Outro permite qualquer periodicidade
            _ => false
        };
    }
}
