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
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateSecurityCommandHandler> _logger;

    public DeactivateSecurityCommandHandler(
        IApplicationDbContext context,
        ILogger<DeactivateSecurityCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeactivateSecurityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating Security with ID: {SecurityId}", request.Id);

        var security = await _context.Securities.FindAsync([request.Id], cancellationToken);
        if (security == null) throw new EntityNotFoundException("Security", request.Id);

        security.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated Security with ID: {SecurityId} at {DeactivatedAt}",
            request.Id, security.DeactivatedAt);
    }
}