using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public class
    GetAllTransactionSubTypesQueryHandler : IRequestHandler<GetAllTransactionSubTypesQuery,
    IReadOnlyList<TransactionSubTypeDto>>
{
    private readonly ILogger<GetAllTransactionSubTypesQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ITransactionSubTypeRepository _repository;

    public GetAllTransactionSubTypesQueryHandler(
        ITransactionSubTypeRepository repository,
        IMapper mapper,
        ILogger<GetAllTransactionSubTypesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionSubTypeDto>> Handle(GetAllTransactionSubTypesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving transaction subtypes{TypeFilter}",
            request.TypeId.HasValue ? $" for type {request.TypeId}" : "");

        var subtypes = request.TypeId.HasValue
            ? await _repository.GetByTypeIdAsync(request.TypeId.Value, cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<TransactionSubTypeDto>>(subtypes);
    }
}