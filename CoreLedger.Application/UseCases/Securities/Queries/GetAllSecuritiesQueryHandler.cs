using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
/// Handler for retrieving all Securities.
/// </summary>
public class GetAllSecuritiesQueryHandler : IRequestHandler<GetAllSecuritiesQuery, IReadOnlyList<SecurityDto>>
{
    private readonly ISecurityRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllSecuritiesQueryHandler> _logger;

    public GetAllSecuritiesQueryHandler(
        ISecurityRepository repository,
        IMapper mapper,
        ILogger<GetAllSecuritiesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SecurityDto>> Handle(
        GetAllSecuritiesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all Securities");

        var securities = await _repository.GetAllAsync(cancellationToken);
        var result = _mapper.Map<IReadOnlyList<SecurityDto>>(securities);

        _logger.LogInformation("Retrieved {Count} Securities", result.Count);

        return result;
    }
}
