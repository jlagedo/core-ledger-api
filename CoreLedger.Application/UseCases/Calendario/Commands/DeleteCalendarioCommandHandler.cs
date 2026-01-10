using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Handler for DeleteCalendarioCommand.
/// </summary>
public class DeleteCalendarioCommandHandler : IRequestHandler<DeleteCalendarioCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteCalendarioCommandHandler> _logger;

    public DeleteCalendarioCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteCalendarioCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeleteCalendarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Calendario with ID {Id}", request.Id);

        var calendario = await _context.Calendarios
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (calendario == null)
        {
            throw new EntityNotFoundException("Calendario", request.Id);
        }

        _context.Calendarios.Remove(calendario);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Calendario with ID {Id} deleted successfully", request.Id);
    }
}
