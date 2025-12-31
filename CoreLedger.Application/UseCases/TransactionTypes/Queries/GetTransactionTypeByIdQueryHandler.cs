using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionTypes.Queries;

public class GetTransactionTypeByIdQueryHandler : IRequestHandler<GetTransactionTypeByIdQuery, TransactionTypeDto>
{
    private readonly ILogger<GetTransactionTypeByIdQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ITransactionTypeRepository _repository;

    public GetTransactionTypeByIdQueryHandler(
        ITransactionTypeRepository repository,
        IMapper mapper,
        ILogger<GetTransactionTypeByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionTypeDto> Handle(GetTransactionTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var type = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (type == null)
            throw new EntityNotFoundException("TransactionType", request.Id);

        return _mapper.Map<TransactionTypeDto>(type);
    }
}