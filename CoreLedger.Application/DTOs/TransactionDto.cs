namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for Transaction entity.
/// </summary>
public record TransactionDto(
    int Id,
    int FundId,
    string FundCode,
    string FundName,
    int? SecurityId,
    string? SecurityTicker,
    string? SecurityName,
    int TransactionSubTypeId,
    string TransactionSubTypeDescription,
    int TransactionTypeId,
    string TransactionTypeDescription,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    int StatusId,
    string StatusDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
///     DTO for creating a new Transaction.
/// </summary>
public record CreateTransactionDto(
    int FundId,
    int? SecurityId,
    int TransactionSubTypeId,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    int StatusId);

/// <summary>
///     DTO for updating an existing Transaction.
/// </summary>
public record UpdateTransactionDto(
    int FundId,
    int? SecurityId,
    int TransactionSubTypeId,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    int StatusId);