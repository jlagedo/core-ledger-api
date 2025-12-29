using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public record GetTransactionSubTypeByIdQuery(int Id) : IRequest<TransactionSubTypeDto>;
