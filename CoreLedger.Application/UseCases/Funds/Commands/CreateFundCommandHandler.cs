using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
/// Handler for creating a new Fund.
/// </summary>
public class CreateFundCommandHandler : IRequestHandler<CreateFundCommand, FundDto>
{
    private readonly IFundRepository _fundRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFundCommandHandler> _logger;

    public CreateFundCommandHandler(
        IFundRepository fundRepository,
        IMapper mapper,
        ILogger<CreateFundCommandHandler> logger)
    {
        _fundRepository = fundRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundDto> Handle(
        CreateFundCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new Fund with name: {Name}", request.Name);

        var existing = await _fundRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existing != null)
        {
            throw new DomainValidationException("Fund with this name already exists");
        }

        var fund = Fund.Create(
            request.Code,
            request.Name,
            request.BaseCurrency,
            request.InceptionDate,
            request.ValuationFrequency);

        var created = await _fundRepository.AddAsync(fund, cancellationToken);

        _logger.LogInformation("Created Fund with ID: {FundId}", created.Id);

        return _mapper.Map<FundDto>(created);
    }
}
