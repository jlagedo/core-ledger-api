using Microsoft.Extensions.Logging;

using AutoMapper;
using MediatR;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Application.UseCases.Transactions.Queries;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly ITransactionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionByIdQueryHandler> _logger;

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
