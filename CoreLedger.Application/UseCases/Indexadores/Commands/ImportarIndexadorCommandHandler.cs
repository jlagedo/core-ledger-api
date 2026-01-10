using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Handler for triggering automatic indexador data import.
/// </summary>
public class ImportarIndexadorCommandHandler : IRequestHandler<ImportarIndexadorCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ImportarIndexadorCommandHandler> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public ImportarIndexadorCommandHandler(
        IApplicationDbContext context,
        IMessagePublisher messagePublisher,
        ILogger<ImportarIndexadorCommandHandler> logger)
    {
        _context = context;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ImportarIndexadorCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Triggering import for indexador {Id}", request.Id);

        var indexador = await _context.Indexadores
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (indexador == null)
        {
            _logger.LogWarning("Import failed: Indexador {Id} not found", request.Id);
            throw new EntityNotFoundException(nameof(Indexador), request.Id);
        }

        // Validate importacao automatica is enabled
        if (!indexador.ImportacaoAutomatica)
        {
            _logger.LogWarning("Import failed: Indexador {Id} does not have automatic import enabled", request.Id);
            throw new DomainValidationException(
                $"Indexador '{indexador.Codigo}' não tem importação automática habilitada");
        }

        // Validate URL fonte exists
        if (string.IsNullOrWhiteSpace(indexador.UrlFonte))
        {
            _logger.LogWarning("Import failed: Indexador {Id} does not have URL fonte configured", request.Id);
            throw new DomainValidationException(
                $"Indexador '{indexador.Codigo}' não tem URL fonte configurada");
        }

        // Publish message to RabbitMQ for Worker processing
        var message = new IndexadorImportMessage(
            indexador.Id,
            indexador.UrlFonte,
            request.CorrelationId);

        await _messagePublisher.PublishAsync(
            "indexador.import",
            message,
            request.CorrelationId,
            cancellationToken);

        _logger.LogInformation(
            "Published import message for indexador {Id} to queue 'indexador.import' with correlation ID {CorrelationId}",
            indexador.Id, request.CorrelationId);

        return Unit.Value;
    }
}
