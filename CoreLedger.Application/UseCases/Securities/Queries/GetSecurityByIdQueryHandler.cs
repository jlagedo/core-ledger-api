
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
/// Handler for retrieving a specific Security by ID.
/// </summary>
public class GetSecurityByIdQueryHandler : IRequestHandler<GetSecurityByIdQuery, SecurityDto>
{
    private readonly ISecurityRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSecurityByIdQueryHandler> _logger;

    public GetSecurityByIdQueryHandler(
        ISecurityRepository repository,
        IMapper mapper,
        ILogger<GetSecurityByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SecurityDto> Handle(
        GetSecurityByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving Security with ID: {SecurityId}", request.Id);

        var security = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (security == null)
        {
            throw new EntityNotFoundException("Security", request.Id);
        }

        var result = _mapper.Map<SecurityDto>(security);

        _logger.LogInformation("Retrieved Security with ID: {SecurityId}", request.Id);

        return result;
    }
}
