namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for TransactionStatus entity.
/// </summary>
public record TransactionStatusDto(
    int Id,
    string ShortDescription,
    string LongDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);