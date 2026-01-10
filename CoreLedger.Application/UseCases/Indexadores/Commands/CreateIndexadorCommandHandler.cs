using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Handler for creating a new Indexador.
/// </summary>
public class CreateIndexadorCommandHandler : IRequestHandler<CreateIndexadorCommand, IndexadorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateIndexadorCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateIndexadorCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateIndexadorCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IndexadorDto> Handle(
        CreateIndexadorCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating indexador {Codigo} - Nome: {Nome}, Tipo: {Tipo}, Periodicidade: {Periodicidade}",
            request.Codigo, request.Nome, request.Tipo, request.Periodicidade);

        // IDX-001: Check for duplicate codigo
        var existing = await _context.Indexadores
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Codigo == request.Codigo.ToUpperInvariant(), cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Indexador creation failed: Duplicate codigo {Codigo} already exists as indexador {ExistingId}",
                request.Codigo, existing.Id);
            throw new DomainValidationException($"Indexador with codigo '{request.Codigo}' already exists");
        }

        var indexador = Indexador.Create(
            request.Codigo,
            request.Nome,
            request.Tipo,
            request.Fonte,
            request.Periodicidade,
            request.FatorAcumulado,
            request.DataBase,
            request.UrlFonte,
            request.ImportacaoAutomatica,
            request.Ativo);

        _context.Indexadores.Add(indexador);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created Indexador with ID: {IndexadorId}", indexador.Id);

        return _mapper.Map<IndexadorDto>(indexador);
    }
}
