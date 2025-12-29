
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
/// Handler for creating a new Security.
/// </summary>
public class CreateSecurityCommandHandler : IRequestHandler<CreateSecurityCommand, SecurityDto>
{
    private readonly ISecurityRepository _securityRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSecurityCommandHandler> _logger;

    public CreateSecurityCommandHandler(
        ISecurityRepository securityRepository,
        IMapper mapper,
        ILogger<CreateSecurityCommandHandler> logger)
    {
        _securityRepository = securityRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SecurityDto> Handle(CreateSecurityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new Security with ticker: {Ticker}", request.Ticker);

        // Check if security with same ticker already exists
        var existing = await _securityRepository.GetByTickerAsync(request.Ticker, cancellationToken);
        if (existing != null)
        {
            throw new DomainValidationException("Security with this ticker already exists");
        }

        var security = Security.Create(
            request.Name,
            request.Ticker,
            request.Isin,
            request.Type,
            request.Currency);

        var created = await _securityRepository.AddAsync(security, cancellationToken);

        _logger.LogInformation("Created Security with ID: {SecurityId}", created.Id);

        return _mapper.Map<SecurityDto>(created);
    }
}
