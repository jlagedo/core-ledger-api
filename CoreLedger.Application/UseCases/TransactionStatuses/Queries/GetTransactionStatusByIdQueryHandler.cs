using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionStatuses.Queries;

public class GetTransactionStatusByIdQueryHandler : IRequestHandler<GetTransactionStatusByIdQuery, TransactionStatusDto>
{
    private readonly ILogger<GetTransactionStatusByIdQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ITransactionStatusRepository _repository;

    public GetTransactionStatusByIdQueryHandler(
        ITransactionStatusRepository repository,
        IMapper mapper,
        ILogger<GetTransactionStatusByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionStatusDto> Handle(GetTransactionStatusByIdQuery request,
        CancellationToken cancellationToken)
    {
        var status = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (status == null)
            throw new EntityNotFoundException("TransactionStatus", request.Id);

        return _mapper.Map<TransactionStatusDto>(status);
    }
}