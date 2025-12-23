using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.AccountTypes.Queries;

/// <summary>
/// Handler for retrieving all AccountType items.
/// </summary>
public class GetAllAccountTypesQueryHandler : IRequestHandler<GetAllAccountTypesQuery, IReadOnlyList<AccountTypeDto>>
{
    private readonly IAccountTypeRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllAccountTypesQueryHandler> _logger;

    public GetAllAccountTypesQueryHandler(
        IAccountTypeRepository repository,
        IMapper mapper,
        ILogger<GetAllAccountTypesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountTypeDto>> Handle(
        GetAllAccountTypesQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all AccountTypes");

        var accountTypes = await _repository.GetAllAsync(cancellationToken);
        var result = _mapper.Map<IReadOnlyList<AccountTypeDto>>(accountTypes);

        _logger.LogInformation("Retrieved {Count} AccountTypes", result.Count);

        return result;
    }
}
