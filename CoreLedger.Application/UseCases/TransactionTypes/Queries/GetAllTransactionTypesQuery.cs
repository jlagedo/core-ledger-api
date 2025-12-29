using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public record GetAllTransactionTypesQuery() : IRequest<IReadOnlyList<TransactionTypeDto>>;
