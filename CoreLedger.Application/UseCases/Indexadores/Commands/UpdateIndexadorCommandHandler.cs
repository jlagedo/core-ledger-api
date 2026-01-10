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
///     Handler for updating an existing Indexador.
/// </summary>
public class UpdateIndexadorCommandHandler : IRequestHandler<UpdateIndexadorCommand, IndexadorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateIndexadorCommandHandler> _logger;
    private readonly IMapper _mapper;

    public UpdateIndexadorCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<UpdateIndexadorCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IndexadorDto> Handle(
        UpdateIndexadorCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating indexador {Id}", request.Id);

        var indexador = await _context.Indexadores
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (indexador == null)
        {
            _logger.LogWarning("Indexador update failed: Indexador {Id} not found", request.Id);
            throw new EntityNotFoundException(nameof(Indexador), request.Id);
        }

        indexador.Update(
            request.Nome,
            request.Tipo,
            request.Fonte,
            request.Periodicidade,
            request.FatorAcumulado,
            request.DataBase,
            request.UrlFonte,
            request.ImportacaoAutomatica,
            request.Ativo);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated Indexador {Id}", indexador.Id);

        return _mapper.Map<IndexadorDto>(indexador);
    }
}
