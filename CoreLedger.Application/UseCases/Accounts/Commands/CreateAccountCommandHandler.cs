using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Handler for creating a new Account.
/// </summary>
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateAccountCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateAccountCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateAccountCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new Account with code: {Code}", request.Code);

        // Validate that the account type exists
        var accountType = await _context.AccountTypes.FindAsync([request.TypeId], cancellationToken);
        if (accountType == null)
        {
            _logger.LogWarning("Account creation failed: AccountType {TypeId} not found", request.TypeId);
            throw new EntityNotFoundException("AccountType", request.TypeId);
        }

        // Check if account with same code already exists
        var existing = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Code == request.Code, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Account creation failed: Duplicate code {Code} already exists as account {ExistingId}", request.Code, existing.Id);
            throw new DomainValidationException("Account with this code already exists");
        }

        var account = Account.Create(
            request.Code,
            request.Name,
            request.TypeId,
            request.Status,
            request.NormalBalance,
            request.CreatedByUserId);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with type for mapping
        var accountWithType = await _context.Accounts
            .Include(a => a.Type)
            .FirstOrDefaultAsync(a => a.Id == account.Id, cancellationToken);

        _logger.LogInformation("Created Account with ID: {AccountId}", account.Id);

        return _mapper.Map<AccountDto>(accountWithType);
    }
}