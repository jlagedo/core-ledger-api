using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Calendario.Commands;

/// <summary>
///     Handler for UpdateCalendarioCommand.
/// </summary>
public class UpdateCalendarioCommandHandler : IRequestHandler<UpdateCalendarioCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateCalendarioCommandHandler> _logger;

    public UpdateCalendarioCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateCalendarioCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UpdateCalendarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Calendario with ID {Id}", request.Id);

        var calendario = await _context.Calendarios
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (calendario == null)
        {
            throw new EntityNotFoundException("Calendario", request.Id);
        }

        // Update using entity method (auto-computes dia_util from tipo_dia per CAL-004)
        calendario.Update(request.TipoDia, request.Descricao);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Calendario with ID {Id} updated successfully", request.Id);
    }
}
