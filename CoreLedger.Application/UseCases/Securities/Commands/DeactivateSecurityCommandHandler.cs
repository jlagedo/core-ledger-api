using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
///     Handler for deactivating a Security.
/// </summary>
public class DeactivateSecurityCommandHandler : IRequestHandler<DeactivateSecurityCommand>
{
    private readonly ILogger<DeactivateSecurityCommandHandler> _logger;
    private readonly ISecurityRepository _securityRepository;

    public DeactivateSecurityCommandHandler(
        ISecurityRepository securityRepository,
        ILogger<DeactivateSecurityCommandHandler> logger)
    {
        _securityRepository = securityRepository;
        _logger = logger;
    }

    public async Task Handle(DeactivateSecurityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating Security with ID: {SecurityId}", request.Id);

        var security = await _securityRepository.GetByIdAsync(request.Id, cancellationToken);
        if (security == null) throw new EntityNotFoundException("Security", request.Id);

        security.Deactivate();
        await _securityRepository.UpdateAsync(security, cancellationToken);

        _logger.LogInformation("Deactivated Security with ID: {SecurityId} at {DeactivatedAt}",
            request.Id, security.DeactivatedAt);
    }
}