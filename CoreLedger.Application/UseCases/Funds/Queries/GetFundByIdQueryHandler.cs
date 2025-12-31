using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Funds.Queries;

/// <summary>
///     Handler for retrieving a specific Fund by ID.
/// </summary>
public class GetFundByIdQueryHandler : IRequestHandler<GetFundByIdQuery, FundDto>
{
    private readonly ILogger<GetFundByIdQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IFundRepository _repository;

    public GetFundByIdQueryHandler(
        IFundRepository repository,
        IMapper mapper,
        ILogger<GetFundByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundDto> Handle(
        GetFundByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving Fund with ID: {FundId}", request.Id);

        var fund = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (fund == null) throw new EntityNotFoundException("Fund", request.Id);

        var result = _mapper.Map<FundDto>(fund);

        _logger.LogInformation("Retrieved Fund with ID: {FundId}", request.Id);

        return result;
    }
}