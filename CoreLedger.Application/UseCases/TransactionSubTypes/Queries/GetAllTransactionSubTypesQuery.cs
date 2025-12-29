using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public record GetAllTransactionSubTypesQuery(int? TypeId = null) : IRequest<IReadOnlyList<TransactionSubTypeDto>>;
