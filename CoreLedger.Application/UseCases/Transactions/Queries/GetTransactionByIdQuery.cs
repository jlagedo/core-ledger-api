using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Transactions.Queries;

public record GetTransactionByIdQuery(int Id) : IRequest<TransactionDto>;
