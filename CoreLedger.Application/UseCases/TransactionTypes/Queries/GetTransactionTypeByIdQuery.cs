using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public record GetTransactionTypeByIdQuery(int Id) : IRequest<TransactionTypeDto>;
