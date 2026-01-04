using CoreLedger.Application.Events;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Extension methods for Transaction entity to support domain event creation.
/// </summary>
public static class TransactionEventExtensions
{
    /// <summary>
    ///     Maps a Transaction entity to TransactionCreatedEvent with denormalized data.
    ///     Requires navigation properties (Fund, Security, TransactionSubType, Status) to be loaded.
    /// </summary>
    /// <param name="transaction">The transaction entity to map.</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing.</param>
    /// <param name="requestId">Optional request ID for request tracking.</param>
    /// <returns>A TransactionCreatedEvent with denormalized data from the transaction and its relationships.</returns>
    public static TransactionCreatedEvent ToTransactionCreatedEvent(
        this Transaction transaction,
        string? correlationId = null,
        string? requestId = null) =>
        new()
        {
            TransactionId = transaction.Id,
            FundId = transaction.FundId,
            FundCode = transaction.Fund?.Code ?? string.Empty,
            FundName = transaction.Fund?.Name ?? string.Empty,
            SecurityId = transaction.SecurityId,
            SecurityTicker = transaction.Security?.Ticker,
            SecurityName = transaction.Security?.Name,
            TransactionSubTypeId = transaction.TransactionSubTypeId,
            TransactionSubTypeDescription = transaction.TransactionSubType?.ShortDescription ?? string.Empty,
            TransactionTypeId = transaction.TransactionSubType?.TypeId ?? 0,
            TransactionTypeDescription = transaction.TransactionSubType?.Type?.ShortDescription ?? string.Empty,
            TradeDate = transaction.TradeDate,
            SettleDate = transaction.SettleDate,
            Quantity = transaction.Quantity,
            Price = transaction.Price,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            StatusId = transaction.StatusId,
            StatusDescription = transaction.Status?.ShortDescription ?? string.Empty,
            CreatedAt = transaction.CreatedAt,
            CreatedByUserId = transaction.CreatedByUserId,
            CorrelationId = correlationId,
            RequestId = requestId,
            OccurredOn = DateTime.UtcNow
        };
}
