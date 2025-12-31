using System.Text.RegularExpressions;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Security domain entity with business rules and invariants.
/// </summary>
public class Security : BaseEntity
{
    private Security()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Ticker { get; private set; } = string.Empty;
    public string? Isin { get; private set; }
    public SecurityType Type { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public SecurityStatus Status { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    /// <summary>
    ///     Identifier of the user who created this security.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Factory method to create a new Security with validation.
    /// </summary>
    public static Security Create(
        string name,
        string ticker,
        string? isin,
        SecurityType type,
        string currency,
        string createdByUserId)
    {
        ValidateName(name);
        ValidateTicker(ticker);
        ValidateIsin(isin);
        ValidateCurrency(currency);
        ValidateCreatedByUserId(createdByUserId);

        return new Security
        {
            Name = name.Trim(),
            Ticker = ticker.Trim().ToUpperInvariant(),
            Isin = isin?.Trim().ToUpperInvariant(),
            Type = type,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = SecurityStatus.Active,
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Updates the security with validation.
    /// </summary>
    public void Update(
        string name,
        string ticker,
        string? isin,
        SecurityType type,
        string currency)
    {
        ValidateName(name);
        ValidateTicker(ticker);
        ValidateIsin(isin);
        ValidateCurrency(currency);

        Name = name.Trim();
        Ticker = ticker.Trim().ToUpperInvariant();
        Isin = isin?.Trim().ToUpperInvariant();
        Type = type;
        Currency = currency.Trim().ToUpperInvariant();
        SetUpdated();
    }

    /// <summary>
    ///     Deactivates the security.
    /// </summary>
    public void Deactivate()
    {
        if (Status == SecurityStatus.Inactive)
            throw new DomainValidationException("Security is already inactive");

        Status = SecurityStatus.Inactive;
        DeactivatedAt = DateTime.UtcNow;
        SetUpdated();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be empty");

        if (name.Length > 200)
            throw new DomainValidationException("Name cannot exceed 200 characters");
    }

    private static void ValidateTicker(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new DomainValidationException("Ticker cannot be empty");

        if (ticker.Length > 20)
            throw new DomainValidationException("Ticker cannot exceed 20 characters");

        if (!Regex.IsMatch(ticker, "^[A-Z0-9-]+$", RegexOptions.IgnoreCase))
            throw new DomainValidationException(
                "Ticker must contain only alphanumeric characters and hyphens (A-Z, 0-9, -)");
    }

    private static void ValidateIsin(string? isin)
    {
        if (isin != null && isin.Trim().Length > 12)
            throw new DomainValidationException("ISIN cannot exceed 12 characters");
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainValidationException("Currency cannot be empty");

        if (currency.Length != 3)
            throw new DomainValidationException("Currency must be a 3-letter ISO code");

        if (!Regex.IsMatch(currency, "^[A-Z]{3}$", RegexOptions.IgnoreCase))
            throw new DomainValidationException("Currency must contain only letters (A-Z)");
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
            throw new DomainValidationException("CreatedByUserId cannot be empty");
    }
}