using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public record CreateTransactionCommand(
    int FundId,
    int? SecurityId,
    int TransactionSubTypeId,
    DateTime TradeDate,
    DateTime SettleDate,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string Currency,
    int StatusId,
    string CreatedByUserId
) : IRequest<TransactionDto>;