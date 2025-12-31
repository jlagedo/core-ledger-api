using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public class
    GetAllTransactionTypesQueryHandler : IRequestHandler<GetAllTransactionTypesQuery, IReadOnlyList<TransactionTypeDto>>
{
    private readonly ILogger<GetAllTransactionTypesQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ITransactionTypeRepository _repository;

    public GetAllTransactionTypesQueryHandler(
        ITransactionTypeRepository repository,
        IMapper mapper,
        ILogger<GetAllTransactionTypesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionTypeDto>> Handle(GetAllTransactionTypesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all transaction types");
        var types = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TransactionTypeDto>>(types);
    }
}