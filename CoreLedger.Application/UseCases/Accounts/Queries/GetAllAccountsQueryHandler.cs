using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

/// <summary>
/// Handler for retrieving all Account items.
/// </summary>
public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, IReadOnlyList<AccountDto>>
{
    private readonly IAccountRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllAccountsQueryHandler> _logger;

    public GetAllAccountsQueryHandler(
        IAccountRepository repository,
        IMapper mapper,
        ILogger<GetAllAccountsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountDto>> Handle(
        GetAllAccountsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all Accounts");

        var accounts = await _repository.GetAllWithTypeAsync(cancellationToken);
        var result = _mapper.Map<IReadOnlyList<AccountDto>>(accounts);

        _logger.LogInformation("Retrieved {Count} Accounts", result.Count);

        return result;
    }
}
