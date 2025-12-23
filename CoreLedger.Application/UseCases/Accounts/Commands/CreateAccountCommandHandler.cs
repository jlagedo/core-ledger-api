using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
/// Handler for creating a new Account.
/// </summary>
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountTypeRepository _accountTypeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        IAccountTypeRepository accountTypeRepository,
        IMapper mapper,
        ILogger<CreateAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _accountTypeRepository = accountTypeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new Account with code: {Code}", request.Code);

        // Validate that the account type exists
        var accountType = await _accountTypeRepository.GetByIdAsync(request.TypeId, cancellationToken);
        if (accountType == null)
        {
            throw new EntityNotFoundException("AccountType", request.TypeId);
        }

        // Check if account with same code already exists
        var existing = await _accountRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing != null)
        {
            throw new DomainValidationException("Account with this code already exists");
        }

        var account = Account.Create(
            request.Code,
            request.Name,
            request.TypeId,
            request.Status,
            request.NormalBalance);

        var created = await _accountRepository.AddAsync(account, cancellationToken);

        // Reload with type for mapping
        var accountWithType = await _accountRepository.GetByIdWithTypeAsync(created.Id, cancellationToken);

        _logger.LogInformation("Created Account with ID: {AccountId}", created.Id);

        return _mapper.Map<AccountDto>(accountWithType);
    }
}
