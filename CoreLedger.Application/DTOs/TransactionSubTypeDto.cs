namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for TransactionSubType entity.
/// </summary>
public record TransactionSubTypeDto(
    int Id,
    int TypeId,
    string TypeDescription,
    string ShortDescription,
    string LongDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
