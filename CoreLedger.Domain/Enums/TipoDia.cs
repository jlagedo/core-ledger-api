namespace CoreLedger.Domain.Enums;

/// <summary>
///     Represents the type of day in the calendar for business day calculations.
/// </summary>
public enum TipoDia
{
    /// <summary>
    ///     Business day (dia útil normal).
    /// </summary>
    Util = 1,

    /// <summary>
    ///     National holiday (feriado nacional).
    /// </summary>
    FeriadoNacional = 2,

    /// <summary>
    ///     State holiday (feriado estadual).
    /// </summary>
    FeriadoEstadual = 3,

    /// <summary>
    ///     Municipal holiday (feriado municipal).
    /// </summary>
    FeriadoMunicipal = 4,

    /// <summary>
    ///     Bank holiday (feriado bancário, e.g., New Year's Eve).
    /// </summary>
    FeriadoBancario = 5,

    /// <summary>
    ///     Weekend (sábado ou domingo).
    /// </summary>
    FimDeSemana = 6,

    /// <summary>
    ///     Optional holiday (ponto facultativo).
    /// </summary>
    PontoFacultativo = 7
}
