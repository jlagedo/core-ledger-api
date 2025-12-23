using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
/// Query to retrieve a specific Account by ID.
/// </summary>
public record GetAccountByIdQuery(int Id) : IRequest<AccountDto>;
