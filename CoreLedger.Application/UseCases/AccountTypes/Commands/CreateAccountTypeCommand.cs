using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
/// Command to create a new AccountType.
/// </summary>
public record CreateAccountTypeCommand(string Description) : IRequest<AccountTypeDto>;
