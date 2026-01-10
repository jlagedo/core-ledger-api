using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     HistoricoIndexador domain entity representing time series data for indexers.
///     Historical data is immutable after creation.
/// </summary>
public class HistoricoIndexador : BaseEntity
{
    private HistoricoIndexador()
    {
    }

    /// <summary>
    ///     Override base Id to use BIGINT for large time series datasets.
    /// </summary>
    public new long Id { get; private set; }

    public int IndexadorId { get; private set; }
    public Indexador? Indexador { get; private set; }
    public DateTime DataReferencia { get; private set; }
    public decimal Valor { get; private set; }
    public decimal? FatorDiario { get; private set; }
    public decimal? VariacaoPercentual { get; private set; }
    public string? Fonte { get; private set; }
    public Guid? ImportacaoId { get; private set; }

    /// <summary>
    ///     Factory method to create a new HistoricoIndexador with validation.
    /// </summary>
    public static HistoricoIndexador Create(
        int indexadorId,
        DateTime dataReferencia,
        decimal valor,
        decimal? fatorDiario,
        decimal? variacaoPercentual,
        string? fonte,
        Guid? importacaoId)
    {
        ValidateIndexadorId(indexadorId);
        ValidateValor(valor);
        ValidateFatorDiario(fatorDiario);

        return new HistoricoIndexador
        {
            IndexadorId = indexadorId,
            DataReferencia = dataReferencia.Date,
            Valor = valor,
            FatorDiario = fatorDiario,
            VariacaoPercentual = variacaoPercentual,
            Fonte = fonte?.Trim(),
            ImportacaoId = importacaoId
        };
    }

    // NO Update method - historical data is immutable

    private static void ValidateIndexadorId(int indexadorId)
    {
        if (indexadorId <= 0)
            throw new DomainValidationException("IndexadorId must be a positive number");
    }

    private static void ValidateValor(decimal valor)
    {
        if (valor < 0)
            throw new DomainValidationException("Valor must be greater than or equal to zero");
    }

    private static void ValidateFatorDiario(decimal? fatorDiario)
    {
        if (fatorDiario.HasValue && fatorDiario.Value <= 0)
            throw new DomainValidationException("Fator diário must be greater than zero");
    }
}
