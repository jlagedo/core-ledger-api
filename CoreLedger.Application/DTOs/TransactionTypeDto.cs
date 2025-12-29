namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for TransactionType entity.
/// </summary>
public record TransactionTypeDto(
    int Id,
    string ShortDescription,
    string LongDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
