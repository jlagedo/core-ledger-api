using Microsoft.Extensions.Logging;

using AutoMapper;
using MediatR;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Application.UseCases.TransactionSubTypes.Queries;

public class GetTransactionSubTypeByIdQueryHandler : IRequestHandler<GetTransactionSubTypeByIdQuery, TransactionSubTypeDto>
{
    private readonly ITransactionSubTypeRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionSubTypeByIdQueryHandler> _logger;

    public GetTransactionSubTypeByIdQueryHandler(
        ITransactionSubTypeRepository repository,
        IMapper mapper,
        ILogger<GetTransactionSubTypeByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionSubTypeDto> Handle(GetTransactionSubTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var subtype = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (subtype == null)
            throw new EntityNotFoundException("TransactionSubType", request.Id);

        return _mapper.Map<TransactionSubTypeDto>(subtype);
    }
}
