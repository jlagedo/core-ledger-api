using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
/// AccountType domain entity with business rules and invariants.
/// </summary>
public class AccountType : BaseEntity
{
    public string Description { get; private set; } = string.Empty;

    private AccountType() { }

    /// <summary>
    /// Factory method to create a new AccountType with validation.
    /// </summary>
    public static AccountType Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Description cannot be empty");

        if (description.Length > 100)
            throw new DomainValidationException("Description cannot exceed 100 characters");

        return new AccountType
        {
            Description = description.Trim()
        };
    }

    /// <summary>
    /// Updates the description with validation.
    /// </summary>
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Description cannot be empty");

        if (description.Length > 100)
            throw new DomainValidationException("Description cannot exceed 100 characters");

        Description = description.Trim();
        SetUpdated();
    }
}
