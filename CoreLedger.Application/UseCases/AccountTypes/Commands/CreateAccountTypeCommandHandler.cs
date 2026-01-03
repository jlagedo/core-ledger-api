using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Handler for creating a new AccountType.
/// </summary>
public class CreateAccountTypeCommandHandler : IRequestHandler<CreateAccountTypeCommand, AccountTypeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateAccountTypeCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateAccountTypeCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateAccountTypeCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountTypeDto> Handle(
        CreateAccountTypeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new AccountType with description: {Description}",
            request.Description);

        // Check if account type with same description already exists
        var existing = await _context.AccountTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(at => at.Description.ToLower() == request.Description.ToLower(), cancellationToken);

        if (existing != null)
            throw new DomainValidationException("Account type with this description already exists");

        var accountType = AccountType.Create(request.Description);

        _context.AccountTypes.Add(accountType);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created AccountType with ID: {AccountTypeId}", accountType.Id);

        return _mapper.Map<AccountTypeDto>(accountType);
    }
}
