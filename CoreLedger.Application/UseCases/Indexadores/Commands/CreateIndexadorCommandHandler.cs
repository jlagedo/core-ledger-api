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
            "Criando indexador {Codigo} - Nome: {Nome}, Tipo: {Tipo}, Periodicidade: {Periodicidade}",
            request.Codigo, request.Nome, request.Tipo, request.Periodicidade);

        // IDX-001: Check for duplicate codigo
        var existing = await _context.Indexadores
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Codigo == request.Codigo.ToUpperInvariant(), cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Falha na criação de indexador: Código duplicado {Codigo} já existe como indexador {ExistingId}",
                request.Codigo, existing.Id);
            throw new DomainValidationException($"Indexador com código '{request.Codigo}' já existe");
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

        _logger.LogInformation("Indexador criado com ID: {IndexadorId}", indexador.Id);

        return _mapper.Map<IndexadorDto>(indexador);
    }
}
