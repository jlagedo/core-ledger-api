using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for Account entity.
/// </summary>
public record AccountDto(
    int Id,
    long Code,
    string Name,
    int TypeId,
    string TypeDescription,
    AccountStatus Status,
    string StatusDescription,
    NormalBalance NormalBalance,
    string NormalBalanceDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeactivatedAt
);

/// <summary>
/// DTO for creating a new Account.
/// </summary>
public record CreateAccountDto(
    long Code,
    string Name,
    int TypeId,
    AccountStatus Status,
    NormalBalance NormalBalance
);

/// <summary>
/// DTO for updating an existing Account.
/// </summary>
public record UpdateAccountDto(
    long Code,
    string Name,
    int TypeId,
    AccountStatus Status,
    NormalBalance NormalBalance
);
