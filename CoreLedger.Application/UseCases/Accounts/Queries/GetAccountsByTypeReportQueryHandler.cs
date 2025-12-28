using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

public class GetAccountsByTypeReportQueryHandler : IRequestHandler<GetAccountsByTypeReportQuery, IReadOnlyList<AccountsByTypeReportDto>>
{
    private readonly IAccountRepository _repository;
    private readonly ILogger<GetAccountsByTypeReportQueryHandler> _logger;

    public GetAccountsByTypeReportQueryHandler(
        IAccountRepository repository,
        ILogger<GetAccountsByTypeReportQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountsByTypeReportDto>> Handle(
        GetAccountsByTypeReportQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving accounts by type report");

        var data = await _repository.GetActiveAccountsByTypeAsync(cancellationToken);

        var result = data.Select(d => new AccountsByTypeReportDto(
            d.TypeId,
            d.TypeDescription,
            d.ActiveAccountCount
        )).ToList();

        _logger.LogInformation("Retrieved report with {Count} account types", result.Count);

        return result;
    }
}
