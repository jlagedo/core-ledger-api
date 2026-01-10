using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Calendario domain entity representing business days and holidays for D+X calculations.
/// </summary>
public class Calendario : BaseEntity
{
    private Calendario() { }

    /// <summary>
    ///     The calendar date.
    /// </summary>
    public DateOnly Data { get; private set; }

    /// <summary>
    ///     Indicates whether the date is a business day.
    ///     Computed automatically from TipoDia (CAL-004).
    /// </summary>
    public bool DiaUtil { get; private set; }

    /// <summary>
    ///     The type of day classification.
    /// </summary>
    public TipoDia TipoDia { get; private set; }

    /// <summary>
    ///     The market location (praça) for this calendar entry.
    /// </summary>
    public Praca Praca { get; private set; }

    /// <summary>
    ///     Optional description (e.g., holiday name).
    /// </summary>
    public string? Descricao { get; private set; }

    /// <summary>
    ///     User ID who created this calendar entry.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Factory method to create a new Calendario with validation.
    /// </summary>
    /// <param name="data">The calendar date.</param>
    /// <param name="tipoDia">The type of day.</param>
    /// <param name="praca">The market location.</param>
    /// <param name="descricao">Optional description (holiday name).</param>
    /// <param name="createdByUserId">User ID who is creating this entry.</param>
    /// <returns>A new Calendario instance.</returns>
    public static Calendario Create(
        DateOnly data,
        TipoDia tipoDia,
        Praca praca,
        string? descricao,
        string createdByUserId)
    {
        ValidateData(data);
        ValidateTipoDia(tipoDia);
        ValidatePraca(praca);
        ValidateDescricao(descricao);
        ValidateCreatedByUserId(createdByUserId);

        return new Calendario
        {
            Data = data,
            TipoDia = tipoDia,
            DiaUtil = ComputeDiaUtil(tipoDia), // CAL-004: Auto-compute from TipoDia
            Praca = praca,
            Descricao = descricao?.Trim(),
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Updates the calendar entry.
    /// </summary>
    /// <param name="tipoDia">The type of day.</param>
    /// <param name="descricao">Optional description.</param>
    public void Update(TipoDia tipoDia, string? descricao)
    {
        ValidateTipoDia(tipoDia);
        ValidateDescricao(descricao);

        TipoDia = tipoDia;
        DiaUtil = ComputeDiaUtil(tipoDia); // CAL-004: Auto-compute from TipoDia
        Descricao = descricao?.Trim();
        SetUpdated();
    }

    /// <summary>
    ///     Computes whether the day is a business day based on TipoDia (CAL-004).
    /// </summary>
    private static bool ComputeDiaUtil(TipoDia tipoDia)
    {
        return tipoDia == TipoDia.Util;
    }

    private static void ValidateData(DateOnly data)
    {
        if (data == default)
        {
            throw new DomainValidationException("Data cannot be default value");
        }

        if (data.Year < 1900 || data.Year > 2100)
        {
            throw new DomainValidationException("Data year must be between 1900 and 2100");
        }
    }

    private static void ValidateTipoDia(TipoDia tipoDia)
    {
        if (!Enum.IsDefined(typeof(TipoDia), tipoDia))
        {
            throw new DomainValidationException("TipoDia must be a valid enum value");
        }
    }

    private static void ValidatePraca(Praca praca)
    {
        if (!Enum.IsDefined(typeof(Praca), praca))
        {
            throw new DomainValidationException("Praca must be a valid enum value");
        }
    }

    private static void ValidateDescricao(string? descricao)
    {
        if (descricao != null && descricao.Length > 100)
        {
            throw new DomainValidationException("Descricao cannot exceed 100 characters");
        }
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            throw new DomainValidationException("CreatedByUserId is required");
        }

        if (createdByUserId.Length > 200)
        {
            throw new DomainValidationException("CreatedByUserId cannot exceed 200 characters");
        }
    }
}
