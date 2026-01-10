namespace CoreLedger.Domain.Enums;

/// <summary>
///     Represents the market location (praça) for calendar reference.
/// </summary>
public enum Praca
{
    /// <summary>
    ///     National calendar (B3/ANBIMA).
    /// </summary>
    Nacional = 1,

    /// <summary>
    ///     São Paulo market location.
    /// </summary>
    SaoPaulo = 2,

    /// <summary>
    ///     Rio de Janeiro market location.
    /// </summary>
    RioDeJaneiro = 3,

    /// <summary>
    ///     US market calendar (for offshore assets).
    /// </summary>
    ExteriorEua = 4,

    /// <summary>
    ///     European market calendar.
    /// </summary>
    ExteriorEur = 5
}
