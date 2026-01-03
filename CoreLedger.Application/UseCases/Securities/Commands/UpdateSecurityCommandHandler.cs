using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Commands;

/// <summary>
///     Handler for updating an existing Security.
/// </summary>
public class UpdateSecurityCommandHandler : IRequestHandler<UpdateSecurityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateSecurityCommandHandler> _logger;

    public UpdateSecurityCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateSecurityCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UpdateSecurityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Security with ID: {SecurityId}", request.Id);

        var security = await _context.Securities.FindAsync([request.Id], cancellationToken);
        if (security == null) throw new EntityNotFoundException("Security", request.Id);

        // Check if another security with the same ticker already exists
        var existing = await _context.Securities
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Ticker == request.Ticker, cancellationToken);
        if (existing != null && existing.Id != request.Id)
            throw new DomainValidationException("Security with this ticker already exists");

        security.Update(
            request.Name,
            request.Ticker,
            request.Isin,
            request.Type,
            request.Currency);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated Security with ID: {SecurityId}", request.Id);
    }
}