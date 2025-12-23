using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
/// Handler for retrieving a specific Account by ID.
/// </summary>
public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IAccountRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAccountByIdQueryHandler> _logger;

    public GetAccountByIdQueryHandler(
        IAccountRepository repository,
        IMapper mapper,
        ILogger<GetAccountByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving Account with ID: {AccountId}", request.Id);

        var account = await _repository.GetByIdWithTypeAsync(request.Id, cancellationToken);
        if (account == null)
        {
            throw new EntityNotFoundException("Account", request.Id);
        }

        var result = _mapper.Map<AccountDto>(account);

        _logger.LogInformation("Retrieved Account with ID: {AccountId}", request.Id);

        return result;
    }
}
