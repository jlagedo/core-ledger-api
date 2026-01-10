using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Queries;

/// <summary>
///     Handler for getting an Indexador by ID.
/// </summary>
public class GetIndexadorByIdQueryHandler : IRequestHandler<GetIndexadorByIdQuery, IndexadorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetIndexadorByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetIndexadorByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetIndexadorByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IndexadorDto> Handle(
        GetIndexadorByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo indexador {Id}", request.Id);

        var indexador = await _context.Indexadores
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (indexador == null)
        {
            _logger.LogWarning("Indexador {Id} não encontrado", request.Id);
            throw new EntityNotFoundException(nameof(Indexador), request.Id);
        }

        return _mapper.Map<IndexadorDto>(indexador);
    }
}
