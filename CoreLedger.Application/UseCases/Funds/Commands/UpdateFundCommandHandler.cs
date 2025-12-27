using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
/// Handler for updating an existing Fund.
/// </summary>
public class UpdateFundCommandHandler : IRequestHandler<UpdateFundCommand>
{
    private readonly IFundRepository _fundRepository;
    private readonly ILogger<UpdateFundCommandHandler> _logger;

    public UpdateFundCommandHandler(
        IFundRepository fundRepository,
        ILogger<UpdateFundCommandHandler> logger)
    {
        _fundRepository = fundRepository;
        _logger = logger;
    }

    public async Task Handle(
        UpdateFundCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Fund with ID: {FundId}", request.Id);

        var fund = await _fundRepository.GetByIdAsync(request.Id, cancellationToken);
        if (fund == null)
        {
            throw new EntityNotFoundException("Fund", request.Id);
        }

        var existingWithName = await _fundRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingWithName != null && existingWithName.Id != request.Id)
        {
            throw new DomainValidationException("Fund with this name already exists");
        }

        fund.Update(
            request.Code,
            request.Name,
            request.BaseCurrency,
            request.InceptionDate,
            request.ValuationFrequency);

        await _fundRepository.UpdateAsync(fund, cancellationToken);

        _logger.LogInformation("Updated Fund with ID: {FundId}", request.Id);
    }
}
