using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
/// Account domain entity with business rules and invariants.
/// </summary>
public class Account : BaseEntity
{
    public long Code { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int TypeId { get; private set; }
    public AccountType? Type { get; private set; }
    public AccountStatus Status { get; private set; }
    public NormalBalance NormalBalance { get; private set; }

    private Account() { }

    /// <summary>
    /// Factory method to create a new Account with validation.
    /// </summary>
    public static Account Create(
        long code,
        string name,
        int typeId,
        AccountStatus status,
        NormalBalance normalBalance)
    {
        ValidateCode(code);
        ValidateName(name);

        return new Account
        {
            Code = code,
            Name = name.Trim(),
            TypeId = typeId,
            Status = status,
            NormalBalance = normalBalance
        };
    }

    /// <summary>
    /// Updates the account with validation.
    /// </summary>
    public void Update(
        long code,
        string name,
        int typeId,
        AccountStatus status,
        NormalBalance normalBalance)
    {
        ValidateCode(code);
        ValidateName(name);

        Code = code;
        Name = name.Trim();
        TypeId = typeId;
        Status = status;
        NormalBalance = normalBalance;
        SetUpdated();
    }

    /// <summary>
    /// Activates the account.
    /// </summary>
    public void Activate()
    {
        if (Status == AccountStatus.Active)
            throw new DomainValidationException("Account is already active");

        Status = AccountStatus.Active;
        SetUpdated();
    }

    /// <summary>
    /// Deactivates the account.
    /// </summary>
    public void Deactivate()
    {
        if (Status == AccountStatus.Inactive)
            throw new DomainValidationException("Account is already inactive");

        Status = AccountStatus.Inactive;
        SetUpdated();
    }

    private static void ValidateCode(long code)
    {
        if (code <= 0)
            throw new DomainValidationException("Code must be a positive number");

        if (code > 9999999999)
            throw new DomainValidationException("Code cannot exceed 10 digits");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be empty");

        if (name.Length > 200)
            throw new DomainValidationException("Name cannot exceed 200 characters");
    }
}
