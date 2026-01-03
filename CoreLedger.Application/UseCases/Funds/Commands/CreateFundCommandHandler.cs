using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
///     Handler for creating a new Fund.
/// </summary>
public class CreateFundCommandHandler : IRequestHandler<CreateFundCommand, FundDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateFundCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateFundCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateFundCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundDto> Handle(
        CreateFundCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new Fund with name: {Name}", request.Name);

        var existing = await _context.Funds
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Name == request.Name, cancellationToken);
        if (existing != null) throw new DomainValidationException("Fund with this name already exists");

        var fund = Fund.Create(
            request.Code,
            request.Name,
            request.BaseCurrency,
            request.InceptionDate,
            request.ValuationFrequency,
            request.CreatedByUserId);

        _context.Funds.Add(fund);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created Fund with ID: {FundId}", fund.Id);

        return _mapper.Map<FundDto>(fund);
    }
}