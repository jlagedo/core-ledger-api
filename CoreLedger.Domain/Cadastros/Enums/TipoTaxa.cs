namespace CoreLedger.Domain.Cadastros.Enums;

/// <summary>
///     Tipo de taxa cobrada pelo fundo de investimento.
/// </summary>
public enum TipoTaxa
{
    /// <summary>
    ///     Taxa de administração.
    /// </summary>
    Administracao = 1,

    /// <summary>
    ///     Taxa de gestão.
    /// </summary>
    Gestao = 2,

    /// <summary>
    ///     Taxa de performance.
    /// </summary>
    Performance = 3,

    /// <summary>
    ///     Taxa de custódia.
    /// </summary>
    Custodia = 4,

    /// <summary>
    ///     Taxa de entrada (aplicação).
    /// </summary>
    Entrada = 5,

    /// <summary>
    ///     Taxa de saída (resgate).
    /// </summary>
    Saida = 6
}
