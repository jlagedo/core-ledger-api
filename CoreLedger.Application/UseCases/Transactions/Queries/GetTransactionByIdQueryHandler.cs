using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Queries;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly ILogger<GetTransactionByIdQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ITransactionRepository _repository;

    public GetTransactionByIdQueryHandler(
        ITransactionRepository repository,
        IMapper mapper,
        ILogger<GetTransactionByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving transaction with ID: {TransactionId}", request.Id);

        var transaction = await _repository.GetByIdWithNavigationAsync(request.Id, cancellationToken);
        if (transaction == null)
            throw new EntityNotFoundException("Transaction", request.Id);

        return _mapper.Map<TransactionDto>(transaction);
    }
}