using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
/// Fund domain entity with business rules and invariants.
/// </summary>
public class Fund : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string BaseCurrency { get; private set; } = string.Empty;
    public DateTime InceptionDate { get; private set; }
    public ValuationFrequency ValuationFrequency { get; private set; }

    private Fund() { }

    /// <summary>
    /// Factory method to create a new Fund with validation.
    /// </summary>
    public static Fund Create(
        string code,
        string name,
        string baseCurrency,
        DateTime inceptionDate,
        ValuationFrequency valuationFrequency)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateBaseCurrency(baseCurrency);
        ValidateInceptionDate(inceptionDate);

        return new Fund
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            BaseCurrency = baseCurrency.Trim().ToUpperInvariant(),
            InceptionDate = inceptionDate.Date,
            ValuationFrequency = valuationFrequency
        };
    }

    /// <summary>
    /// Updates the fund with validation.
    /// </summary>
    public void Update(
        string code,
        string name,
        string baseCurrency,
        DateTime inceptionDate,
        ValuationFrequency valuationFrequency)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateBaseCurrency(baseCurrency);
        ValidateInceptionDate(inceptionDate);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        BaseCurrency = baseCurrency.Trim().ToUpperInvariant();
        InceptionDate = inceptionDate.Date;
        ValuationFrequency = valuationFrequency;
        SetUpdated();
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainValidationException("Fund code cannot be empty");

        if (code.Length > 10)
            throw new DomainValidationException("Fund code cannot exceed 10 characters");

        if (!System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Z0-9]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            throw new DomainValidationException("Fund code must contain only alphanumeric characters (A-Z, 0-9)");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Fund name cannot be empty");

        if (name.Length > 200)
            throw new DomainValidationException("Fund name cannot exceed 200 characters");
    }

    private static void ValidateBaseCurrency(string baseCurrency)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency))
            throw new DomainValidationException("Base currency cannot be empty");

        if (baseCurrency.Length != 3)
            throw new DomainValidationException("Base currency must be a 3-letter ISO code");
    }

    private static void ValidateInceptionDate(DateTime inceptionDate)
    {
        if (inceptionDate > DateTime.UtcNow.Date)
            throw new DomainValidationException("Inception date cannot be in the future");
    }
}
