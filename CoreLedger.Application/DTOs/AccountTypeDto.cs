namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for AccountType entity.
/// </summary>
public record AccountTypeDto(
    int Id,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO for creating a new AccountType.
/// </summary>
public record CreateAccountTypeDto(string Description);

/// <summary>
/// DTO for updating an existing AccountType.
/// </summary>
public record UpdateAccountTypeDto(string Description);
