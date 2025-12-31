using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Queries;

/// <summary>
///     Handler for retrieving a specific AccountType by ID.
/// </summary>
public class GetAccountTypeByIdQueryHandler : IRequestHandler<GetAccountTypeByIdQuery, AccountTypeDto>
{
    private readonly ILogger<GetAccountTypeByIdQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IAccountTypeRepository _repository;

    public GetAccountTypeByIdQueryHandler(
        IAccountTypeRepository repository,
        IMapper mapper,
        ILogger<GetAccountTypeByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountTypeDto> Handle(
        GetAccountTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving AccountType with ID: {AccountTypeId}", request.Id);

        var accountType = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (accountType == null) throw new EntityNotFoundException("AccountType", request.Id);

        var result = _mapper.Map<AccountTypeDto>(accountType);

        _logger.LogInformation("Retrieved AccountType with ID: {AccountTypeId}", request.Id);

        return result;
    }
}