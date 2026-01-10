using System.Text.RegularExpressions;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Indexador domain entity representing economic indicators used as benchmarks.
/// </summary>
public class Indexador : BaseEntity
{
    private Indexador()
    {
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public IndexadorTipo Tipo { get; private set; }
    public string? Fonte { get; private set; }
    public Periodicidade Periodicidade { get; private set; }
    public decimal? FatorAcumulado { get; private set; }
    public DateTime? DataBase { get; private set; }
    public string? UrlFonte { get; private set; }
    public bool ImportacaoAutomatica { get; private set; }
    public bool Ativo { get; private set; }

    /// <summary>
    ///     Factory method to create a new Indexador with validation.
    /// </summary>
    public static Indexador Create(
        string codigo,
        string nome,
        IndexadorTipo tipo,
        string? fonte,
        Periodicidade periodicidade,
        decimal? fatorAcumulado,
        DateTime? dataBase,
        string? urlFonte,
        bool importacaoAutomatica,
        bool ativo)
    {
        ValidateCodigo(codigo);
        ValidateNome(nome);
        ValidateFatorAcumulado(fatorAcumulado, dataBase);
        ValidateUrlFonte(urlFonte, importacaoAutomatica);
        ValidatePeriodicidadeCompatibility(tipo, periodicidade);

        return new Indexador
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Nome = nome.Trim(),
            Tipo = tipo,
            Fonte = fonte?.Trim(),
            Periodicidade = periodicidade,
            FatorAcumulado = fatorAcumulado,
            DataBase = dataBase?.Date,
            UrlFonte = urlFonte?.Trim(),
            ImportacaoAutomatica = importacaoAutomatica,
            Ativo = ativo
        };
    }

    /// <summary>
    ///     Updates the indexador with validation.
    /// </summary>
    public void Update(
        string nome,
        IndexadorTipo tipo,
        string? fonte,
        Periodicidade periodicidade,
        decimal? fatorAcumulado,
        DateTime? dataBase,
        string? urlFonte,
        bool importacaoAutomatica,
        bool ativo)
    {
        ValidateNome(nome);
        ValidateFatorAcumulado(fatorAcumulado, dataBase);
        ValidateUrlFonte(urlFonte, importacaoAutomatica);
        ValidatePeriodicidadeCompatibility(tipo, periodicidade);

        Nome = nome.Trim();
        Tipo = tipo;
        Fonte = fonte?.Trim();
        Periodicidade = periodicidade;
        FatorAcumulado = fatorAcumulado;
        DataBase = dataBase?.Date;
        UrlFonte = urlFonte?.Trim();
        ImportacaoAutomatica = importacaoAutomatica;
        Ativo = ativo;
        SetUpdated();
    }

    /// <summary>
    ///     Activates the indexador.
    /// </summary>
    public void Activate()
    {
        if (Ativo)
            throw new DomainValidationException("Indexador is already active");

        Ativo = true;
        SetUpdated();
    }

    /// <summary>
    ///     Deactivates the indexador.
    /// </summary>
    public void Deactivate()
    {
        if (!Ativo)
            throw new DomainValidationException("Indexador is already inactive");

        Ativo = false;
        SetUpdated();
    }

    /// <summary>
    ///     Updates the accumulated factor with validation.
    /// </summary>
    public void UpdateFatorAcumulado(decimal fator, DateTime dataBase)
    {
        if (fator <= 0)
            throw new DomainValidationException("Fator acumulado must be greater than zero");

        FatorAcumulado = fator;
        DataBase = dataBase.Date;
        SetUpdated();
    }

    private static void ValidateCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainValidationException("Código do indexador cannot be empty");

        if (codigo.Length > 20)
            throw new DomainValidationException("Código do indexador cannot exceed 20 characters");

        if (!Regex.IsMatch(codigo, "^[A-Z0-9]+$", RegexOptions.IgnoreCase))
            throw new DomainValidationException(
                "Código do indexador must contain only alphanumeric characters (A-Z, 0-9)");
    }

    private static void ValidateNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainValidationException("Nome do indexador cannot be empty");

        if (nome.Length > 100)
            throw new DomainValidationException("Nome do indexador cannot exceed 100 characters");
    }

    private static void ValidateFatorAcumulado(decimal? fatorAcumulado, DateTime? dataBase)
    {
        if (fatorAcumulado.HasValue)
        {
            if (fatorAcumulado.Value <= 0)
                throw new DomainValidationException("Fator acumulado must be greater than zero");

            if (!dataBase.HasValue)
                throw new DomainValidationException("Data base is required when fator acumulado is provided");
        }
    }

    private static void ValidateUrlFonte(string? urlFonte, bool importacaoAutomatica)
    {
        // IDX-003: Importação automática requer URL
        if (importacaoAutomatica && string.IsNullOrWhiteSpace(urlFonte))
            throw new DomainValidationException(
                "URL fonte é obrigatória quando importação automática está habilitada");
    }

    private static void ValidatePeriodicidadeCompatibility(IndexadorTipo tipo, Periodicidade periodicidade)
    {
        // IDX-004: Periodicidade deve ser compatível com tipo
        var isCompatible = tipo switch
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

        if (!isCompatible)
            throw new DomainValidationException(
                $"Periodicidade {periodicidade} não é compatível com o tipo {tipo}");
    }
}
