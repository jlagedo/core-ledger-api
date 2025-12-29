using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.TransactionStatuses.Queries;

public record GetTransactionStatusByIdQuery(int Id) : IRequest<TransactionStatusDto>;
